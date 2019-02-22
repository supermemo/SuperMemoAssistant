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
// Created On:   2019/02/22 19:04
// Modified On:  2019/02/22 19:29
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.PluginHost;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Interop.Plugins
{
  public partial class PluginHost : SMMarshalByRefObject, IDisposable
  {
    #region Properties & Fields - Non-Public
    
    private readonly ISMAPlugin _plugin;

    private bool _hasExited;

    #endregion




    #region Constructors

    public PluginHost(
      string  pluginPackageName,
      Process smaProcess,
      string  smaChannelName,
      bool    isDev)
    {
      // Connect to SMA
      var pluginMgr = RemotingServicesEx.ConnectToIpcServer<ISMAPluginManager>(smaChannelName);

      if (pluginMgr == null)
      {
        Exit(HostConst.ExitIpcConnectionError);
        return;
      }

      // Get required assemblies name
      IEnumerable<string> pluginAssemblies;
      IEnumerable<string> dependenciesAssemblies;

      if (isDev)
      {
        var homePath = new DirectoryPath(AppDomain.CurrentDomain.BaseDirectory);
        var pluginFilePath = homePath.CombineFile(pluginPackageName + ".dll");

        pluginAssemblies = new List<string>
        {
          pluginFilePath.FullPath
        };
        dependenciesAssemblies = new List<string>();
      }

      else if (pluginMgr.GetAssembliesPathsForPlugin(
        pluginPackageName,
        out pluginAssemblies,
        out dependenciesAssemblies) == false)
      {
        Exit(HostConst.ExitCouldNotGetAssembliesPaths);
        return;
      }

      // Load & create plugin
      _plugin = LoadAssembliesAndCreatePluginInstance(
        dependenciesAssemblies,
        pluginAssemblies);

      if (_plugin == null)
      {
        Exit(HostConst.ExitNoPluginTypeFound);
        return;
      }

      // Connect plugin to SMA
      var sma = pluginMgr.ConnectPlugin(_plugin.ChannelName, Process.GetCurrentProcess().Id);

      if (sma == null)
      {
        Exit(HostConst.ExitCouldNotConnectPlugin);
        return;
      }

      // Inject properties
      InjectPropertyDependencies(_plugin, sma, pluginMgr);

      _plugin.OnInjected();

      // Start monitoring SMA process
      if (StartMonitoringSMAProcess(smaProcess) == false)
        Exit(HostConst.ExitParentExited);
    }


    /// <inheritdoc />
    public void Dispose()
    {
      _plugin?.Dispose();
    }

    #endregion




    #region Methods

    private bool StartMonitoringSMAProcess(Process smaProc)
    {
      if (smaProc.HasExited)
        return false;

      Task.Factory.StartNew(MonitorSMAProcess,
                            smaProc,
                            TaskCreationOptions.LongRunning);

      smaProc.Exited += (o,
                         ev) => OnSMAStopped();

      return true;
    }

    private void MonitorSMAProcess(object param)
    {
      Process smaProc = (Process)param;

      while (_hasExited == false && smaProc.HasExited == false)
        smaProc.Refresh();

      if (smaProc.HasExited)
        OnSMAStopped();
    }

    private void OnSMAStopped()
    {
      Exit(0);
    }

    private void Exit(int code)
    {
      _hasExited = true;

      Environment.Exit(code);
    }

    #endregion
  }
}
