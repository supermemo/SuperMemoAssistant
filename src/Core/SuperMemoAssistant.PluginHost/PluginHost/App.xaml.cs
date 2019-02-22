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
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/22 19:31
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

// ReSharper disable HeuristicUnreachableCode

namespace SuperMemoAssistant.PluginHost
{
  /// <summary>Interaction logic for App.xaml</summary>
  public partial class App : Application
  {
    #region Properties & Fields - Non-Public

    private IDisposable _pluginHost;

    #endregion




    #region Methods Impl

    protected override void OnExit(ExitEventArgs e)
    {
      _pluginHost.Dispose();

      base.OnExit(e);
    }

    #endregion




    #region Methods

    private void Application_Startup(object           sender,
                                     StartupEventArgs e)
    {
      try
      {
        Debugger.Launch();
        Process smaProc;

        if (ReadArgs(e.Args,
                     out var pluginPackageName,
                     out var smaProcId,
                     out var channelName,
                     out var homeDir,
                     out var isDev) == false)
        {
          Environment.Exit(HostConst.ExitParameters);
          return;
        }

        try
        {
          smaProc = Process.GetProcessById(smaProcId);
        }
        catch (Exception)
        {
          Environment.Exit(HostConst.ExitParentExited);
          return;
        }

        _pluginHost = PluginLoader.Create(pluginPackageName, smaProc, channelName, homeDir, isDev);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Environment.Exit(HostConst.ExitUnknownError);
      }
    }


    private bool ReadArgs(string[]   args,
                          out string pluginPackageName,
                          out int    smaProcId,
                          out string channelName,
                          out string homeDir,
                          out bool   isDev)
    {
      pluginPackageName = null;
      smaProcId         = -1;
      channelName       = null;
      homeDir           = null;
      isDev             = args.Length == 5 && args[4] == "--development";

      if (args.Length < 4 || args.Length > 5 || args.Any(string.IsNullOrWhiteSpace))
        return false;

      pluginPackageName = args[0];
      channelName       = args[2];
      homeDir           = args[3];

      return int.TryParse(args[1], out smaProcId);
    }

    #endregion
  }
}
