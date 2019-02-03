#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2019/01/19 06:01
// Modified On:  2019/01/25 23:42
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SuperMemoAssistant.Interop.Plugins;

namespace SuperMemoAssistant.PluginHost
{
  /// <summary>Interaction logic for App.xaml</summary>
  public partial class App : Application
  {
    private const int ExitParameters = 1;
    private const int ExitUnknownError = 2;
    private const int ExitParentExited = 3;
    private const int ExitIpcConnectionError = 4;
    private const int ExitCouldNotGetAssembliesPaths = 5;
    private const int ExitNoPluginTypeFound = 6;
    private const int ExitCouldNotConnectPlugin = 7;

    #region Methods

    private void Application_Startup(object           sender,
                                     StartupEventArgs e)
    {
      try
      {
        if (ReadArgs(e.Args, out var pluginPackageName, out var smaProcId, out var channelName) == false)
        {
          Exit(ExitParameters);
          return;
        }
        
        if (StartMonitoringSMAProcess(Process.GetProcessById(smaProcId)) == false)
        {
          Exit(ExitParentExited);
          return;
        }

        var pluginMgr = ConnectToIpcServer(channelName);

        if (pluginMgr == null)
        {
          Exit(ExitIpcConnectionError);
          return;
        }

        // TODO: AppDomain

        if (pluginMgr.GetAssembliesPathsForPlugin(
          pluginPackageName,
          out var pluginAssemblies,
          out var dependenciesAssemblies) == false)
        {
          Exit(ExitCouldNotGetAssembliesPaths);
          return;
        }

        var plugin = PluginLoader.LoadAssembliesAndCreatePluginInstance(
          dependenciesAssemblies,
          pluginAssemblies);

        if (plugin == null)
        {
          Exit(ExitNoPluginTypeFound);
          return;
        }

        var sma = pluginMgr.ConnectPlugin(plugin, Process.GetCurrentProcess().Id);

        if (sma == null)
        {
          Exit(ExitCouldNotConnectPlugin);
          return;
        }

        PluginLoader.InjectPropertyDependencies(plugin, sma, pluginMgr);

        // TODO: Logger, Sentry, etc.
      }
      catch
      {
        Exit(ExitUnknownError);
      }
      
      // Monitor parentPID
      // new IpcChannel(null, null, null)
      // Request assemblies for assemblyName
      // AppDomain
      // AssemblyLoader
      // Plugin loader
    }

    private ISMAPluginManager ConnectToIpcServer(string channelName)
    {
      return (ISMAPluginManager)Activator.GetObject(
        typeof(ISMAPluginManager),
        "ipc://" + channelName + "/" + channelName);
    }

    private bool StartMonitoringSMAProcess(Process smaProc)
    {  
      if (smaProc.HasExited)
        return false;

      Task.Factory.StartNew(MonitorSMAProcess,
                            smaProc,
                            TaskCreationOptions.LongRunning);
      
      smaProc.Exited += (o, ev) => OnSMAStopped();

      return true;
    }

    private void MonitorSMAProcess(object param)
    {
      Process smaProc = (Process)param;

      while (HasExited == false && smaProc.HasExited == false)
      {
        smaProc.Refresh();
      }

      if (smaProc.HasExited)
        OnSMAStopped();
    }

    private void OnSMAStopped()
    {
      Exit(0);
    }

    private new void Exit(int code)
    {
      HasExited = true;
      
      Environment.Exit(code);
    }

    private bool HasExited { get; set; }

    private bool ReadArgs(string[]   args,
                          out string pluginPackageName,
                          out int    smaProcId,
                          out string channelName)
    {
      pluginPackageName = null;
      smaProcId    = -1;
      channelName = null;

      if (args.Length != 3 || args.Any(string.IsNullOrWhiteSpace))
        return false;

      pluginPackageName = args[0];
      channelName = args[2];

      return int.TryParse(args[1], out smaProcId);
    }

    #endregion
  }
}
