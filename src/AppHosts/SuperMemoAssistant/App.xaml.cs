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
// Created On:   2020/01/22 09:58
// Modified On:  2020/01/22 17:31
// Modified By:  Alexis

#endregion




using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using CommandLine;
using Forge.Forms;
using Hardcodet.Wpf.TaskbarNotification;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.PluginHost;
using SuperMemoAssistant.SMA.Utils;
using SuperMemoAssistant.Sys.IO;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant
{
  /// <summary>Interaction logic for App.xaml</summary>
  public partial class App : Application
  {
    #region Properties & Fields - Non-Public

    private TaskbarIcon _taskbarIcon;

    #endregion




    #region Methods Impl

    protected override void OnExit(ExitEventArgs e)
    {
      _taskbarIcon?.Dispose();

      SuperMemoAssistant.SMA.Core.Logger?.Shutdown();
      ModuleInitializer.SentryInstance?.Dispose();

      base.OnExit(e);
    }

    #endregion




    #region Methods

    private async void Application_Startup(object           o,
                                           StartupEventArgs e)
    {
      if (Parser.Default.ParseArguments<SMAParameters>(e.Args) is Parsed<SMAParameters> parsed)
        await LoadApp(parsed.Value);

      else
        Environment.Exit(HostConst.ExitParameters);
    }

    private async Task LoadApp(SMAParameters args)
    {
      _taskbarIcon = (TaskbarIcon)FindResource("TbIcon");

      if (CheckAssemblies(out var errMsg) == false || CheckSMALocation(out errMsg) == false)
      {
        LogTo.Warning(errMsg);
        await Show.Window().For(new Alert(errMsg, "Error"));

        Shutdown();
        return;
      }

      if (args.KeyLogger)
        SuperMemoAssistant.SMA.Core.KeyboardHotKey.MainCallback = LogHotKeys;

      SMCollection smCollection = null;
      var          selectionWdw = new CollectionSelectionWindow();

      // Try to open command line collection, if one was passed
      if (args.CollectionKnoPath != null && selectionWdw.ValidateSuperMemoPath())
      {
        smCollection = new SMCollection(args.CollectionKnoPath, DateTime.Now);

        if (selectionWdw.ValidateCollection(smCollection) == false)
          smCollection = null;
      }

      // No valid collection passed, show selection window
      if (smCollection == null)
      {
        selectionWdw.ShowDialog();

        smCollection = selectionWdw.Collection;
      }

      // If a collection was selected, start SMA
      if (smCollection != null)
      {
        SuperMemoAssistant.SMA.Core.SMA.OnSMStoppedEvent += Instance_OnSMStoppedEvent;

        if (await SuperMemoAssistant.SMA.Core.SMA.Start(smCollection).ConfigureAwait(true) == false)
        {
          await Show.Window().For(
            new Alert(
              $"SMA failed to start. Please check the logs in '{SMAFileSystem.LogDir.FullPath}' for details.",
              "Error")
          );
          Shutdown();
        }
      }
      else
      {
        Shutdown();
      }
    }

    private Task Instance_OnSMStoppedEvent(object        sender,
                                           SMProcessArgs e)
    {
      return Dispatcher.InvokeAsync(Shutdown).Task;
    }

    private bool CheckSMALocation(out string error)
    {
      var smaExeFile = new FilePath(Assembly.GetExecutingAssembly().Location);

      error = null;

      if (smaExeFile.Directory == SMAFileSystem.AppRootDir)
        return true;

      error = $"SuperMemoAssistant should be located in the '{SMAFileSystem.AppRootDir}' folder";

      return false;
    }

    private bool CheckAssemblies(out string error)
    {
      if (AssemblyCheck.CheckFasm32(out error) == false
        || AssemblyCheck.CheckMshtml(out error) == false)
        return false;

      return true;
    }

    private void LogHotKeys(HotKey hk)
    {
      LogTo.Debug($"Key pressed: {hk}");
    }

    #endregion
  }
}
