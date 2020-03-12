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
// Modified On:  2020/02/17 21:40
// Modified By:  Alexis

#endregion




using System;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using CommandLine;
using Hardcodet.Wpf.TaskbarNotification;
using SuperMemoAssistant.Installer;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Models;
using SuperMemoAssistant.Services.UI.Extensions;
using SuperMemoAssistant.Setup;
using SuperMemoAssistant.SMA.Utils;
using SuperMemoAssistant.Sys.Windows;
using SuperMemoAssistant.UI;

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

      SMA.Core.Logger?.Shutdown();
#pragma warning disable CS0436 // Type conflicts with imported type
      ModuleInitializer.SentryInstance?.Dispose();
#pragma warning restore CS0436 // Type conflicts with imported type

      base.OnExit(e);
    }

    #endregion




    #region Methods

    private async void Application_Startup(object           o,
                                           StartupEventArgs e)
    {
      _taskbarIcon = (TaskbarIcon)FindResource("TbIcon");

      if (Parser.Default.ParseArguments<SMAParameters>(e.Args) is Parsed<SMAParameters> parsed)
        await LoadApp(parsed.Value);

      else
        Shutdown(SMAExitCodes.ExitCodeParametersError);
    }

    private async Task LoadApp(SMAParameters args)
    {
      //
      // Installer events
      if (SMAInstaller.HandleEvent(args, out var firstRun))
      {
        if (firstRun)
          MessageBox.Show("SuperMemo Assistant has been successfully installed.", "Installation success");

        Shutdown();
        return;
      }

      //
      // Make sure assemblies are available, and SMA is installed in "%LocalAppData%\SuperMemoAssistant"
      if (AssemblyCheck.CheckRequired(out var errMsg) == false || CheckSMALocation(out errMsg) == false)
      {
        LogTo.Warning(errMsg);
        await errMsg.ErrorMsgBox();

        Shutdown(SMAExitCodes.ExitCodeDependencyError);
        return;
      }

      //
      // Load system configs
      if (await LoadConfigs(out var nativeDataCfg, out var coreCfg) == false)
      {
        errMsg =
          $"At least one essential config file could not be loaded: nativeDataCfg ? {nativeDataCfg == null} ; startupCfg ? {coreCfg == null}";
        LogTo.Warning(errMsg);
        await errMsg.ErrorMsgBox();

        Shutdown(SMAExitCodes.ExitCodeConfigError);
        return;
      }
      
      SMA.Core.CoreConfig = coreCfg;

      //
      // Setup toast notifications (TODO: setup ToastActivatorCLSID on shortcut https://github.com/WindowsNotifications/desktop-toasts/blob/472a3f9f5849fbc62bf5cad769421d4299c47f51/CS/DesktopToastsSetupProject/Product.wxs)
      DesktopNotificationManager.RegisterAumidAndComServer<SMANotificationActivator>("SuperMemoAssistant");
      DesktopNotificationManager.RegisterActivator<SMANotificationActivator>();

      //
      // Check if SMA is setup, and run the setup wizard if it isn't
      if (SMASetup.Run(nativeDataCfg, coreCfg) == false)
      {
        LogTo.Warning("SMA Setup canceled. Exiting.");

        Shutdown(SMAExitCodes.ExitCodeSMASetupError);
        return;
      }

      //
      // (Optional) Start the debug Key logger (logs key strokes with modifiers, e.g. ctrl, alt, ..)
      if (args.KeyLogger)
        SMA.Core.KeyboardHotKey.MainCallback = hk => LogTo.Debug($"Key pressed: {hk}");

      //
      // Determine which collection to open
      SMCollection smCollection = null;
      var          selectionWdw = new CollectionSelectionWindow(coreCfg);

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

      //
      // If a collection was defined, start SMA
      if (smCollection != null)
      {
        SMA.Core.SMA.OnSMStoppedEvent += OnSMStoppedEvent;

        if (await SMA.Core.SMA.Start(nativeDataCfg, smCollection).ConfigureAwait(true) == false)
        {
          await $"SMA failed to start. Please check the logs in '{SMAFileSystem.LogDir.FullPath}' for details.".ErrorMsgBox();
          Shutdown(SMAExitCodes.ExitCodeSMAStartupError);

          return;
        }

        if (SMAExecutableInfo.Instance.IsDev == false)
          await SMAInstaller.Instance.Update();
      }
      else
      {
        Shutdown();
      }
    }

    private void OnSMStoppedEvent(object sender, SMProcessArgs e)
    {
      try
      {
        LogTo.Debug($"Cleaning up {GetType().Name}");

        Dispatcher.Invoke(Shutdown);
      }
      finally
      {
        LogTo.Debug($"Cleaning up {GetType().Name}... Done");
      }
    }

    private bool CheckSMALocation(out string error)
    {
      error = null;

      if (SMAExecutableInfo.Instance.IsPathLocalAppData)
        return true;

      error = $"SuperMemoAssistant should be located in the '{SMAFileSystem.AppRootDir}\\app-x.y.z.b' folder";

      return false;
    }

    #endregion
  }
}
