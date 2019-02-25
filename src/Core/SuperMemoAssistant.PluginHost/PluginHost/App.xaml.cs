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
using System.Windows;
using CommandLine;

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
        Parser.Default.ParseArguments<PluginHostParameters>(e.Args)
              .WithParsed(LoadPlugin)
              .WithNotParsed(_ => Environment.Exit(HostConst.ExitParameters));
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Environment.Exit(HostConst.ExitUnknownError);
      }
    }

    private void LoadPlugin(PluginHostParameters args)
    {
      Process smaProcess;
        
      try
      {
        smaProcess = Process.GetProcessById(args.SMAProcessId);
      }
      catch (Exception)
      {
        Environment.Exit(HostConst.ExitParentExited);
        return;
      }

      _pluginHost = PluginLoader.Create(
        args.PackageName,
        args.HomePath,
        args.Guid,
        args.SMAChannelName,
        smaProcess,
        args.IsDeveloment);
    }

    #endregion
  }
}
