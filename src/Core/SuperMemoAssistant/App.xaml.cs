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

#endregion




namespace SuperMemoAssistant
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Threading.Tasks;
  using System.Windows;
  using Anotar.Serilog;
  using CommandLine;
  using Hardcodet.Wpf.TaskbarNotification;
  using Installer;
  using Interop;
  using Interop.SuperMemo.Core;
  using Models;
  using Plugins;
  using Services.UI.Extensions;
  using Setup;
  using SMA.Utils;
  using Sys.Windows;
  using UI;
  using Utils;

  /// <summary>Interaction logic for App.xaml</summary>
  public partial class App : Application
  {
    #region Properties & Fields - Non-Public

    private SplashScreenWindow _splashScreen;

    private TaskbarIcon _taskbarIcon;

    #endregion




    #region Methods Impl

    /// <summary>The application main stopping point</summary>
    /// <param name="e"></param>
    protected override void OnExit(ExitEventArgs e)
    {
      SMAInstaller.Instance.Semaphore.Wait();

      _taskbarIcon?.Dispose();
      _splashScreen?.Close();
      _splashScreen = null;

      SMA.Core.Logger?.Shutdown();
#pragma warning disable CS0436 // Type conflicts with imported type
      ModuleInitializer.SentryInstance?.Dispose();
#pragma warning restore CS0436 // Type conflicts with imported type

      base.OnExit(e);
    }

    #endregion




    #region Methods

    /// <summary>The application main starting point</summary>
    /// <param name="o"></param>
    /// <param name="e"></param>
    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "WPF API")]
    private async void Application_Startup(object           o,
                                           StartupEventArgs e)
    {
      _taskbarIcon = (TaskbarIcon)FindResource("TbIcon");

      if (ApplicationSingleton.IsOnlyInstance(_ => LogTo.Warning("SuperMemoAssistant is already running. Exiting."), e.Args) == false)
      {
        Shutdown(SMAExitCodes.ExitCodeSMAAlreadyRunning);
        return;
      }

      try
      {
        if (Parser.Default.ParseArguments<SMAParameters>(e.Args) is Parsed<SMAParameters> parsed)
          await LoadAppAsync(parsed.Value).ConfigureAwait(false);

        else
          Shutdown(SMAExitCodes.ExitCodeParametersError);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "An unknown exception occured during SMA Application startup");

        Shutdown(SMAExitCodes.ExitCodeSMAStartupError);
      }
    }

    private async Task LoadAppAsync(SMAParameters args)
    {
      //
      // Installer events https://github.com/Squirrel/Squirrel.Windows/blob/master/docs/using/custom-squirrel-events.md
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
        await errMsg.ErrorMsgBox().ConfigureAwait(false);

        Shutdown(SMAExitCodes.ExitCodeDependencyError);
        return;
      }

      //
      // Load main configuration files
      var (success, nativeDataCfg, coreCfg) = await LoadConfigs().ConfigureAwait(true);

      if (success == false)
      {
        errMsg =
          $"At least one essential config file could not be loaded: nativeDataCfg ? {nativeDataCfg == null} ; startupCfg ? {coreCfg == null}";
        LogTo.Warning(errMsg);
        await errMsg.ErrorMsgBox().ConfigureAwait(false);

        Shutdown(SMAExitCodes.ExitCodeConfigError);
        return;
      }

      SMA.Core.CoreConfig = coreCfg;

      //
      // Setup Windows Toast notifications
      DesktopNotificationManager.RegisterAumidAndComServer<SMANotificationActivator>(SMANotificationActivator.AppUserModelId);
      DesktopNotificationManager.RegisterActivator<SMANotificationActivator>();

      //
      // Initialize the plugin manager
      await SMAPluginManager.Instance.InitializeAsync().ConfigureAwait(true);

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
        SMA.Core.KeyboardHotKey.MainCallback = hk => LogTo.Debug("Key pressed: {Hk}", hk);

      //
      // Show the change logs if necessary
      ChangeLogWindow.ShowIfUpdated(coreCfg);

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
      // If a collection was selected, start SMA
      if (smCollection != null)
      {
        _splashScreen = new SplashScreenWindow();
        _splashScreen.Show();

        SMA.Core.SMA.OnSMStartedEvent += OnSMStartedEventAsync;
        SMA.Core.SMA.OnSMStoppedEvent += OnSMStoppedEvent;

        Exception ex;

        if ((ex = await SMA.Core.SMA.StartAsync(nativeDataCfg, smCollection).ConfigureAwait(true)) != null)
        {
          _splashScreen?.Close();
          _splashScreen = null;

          await $"SMA failed to start: {ex.Message}".ErrorMsgBox().ConfigureAwait(false);

          Shutdown(SMAExitCodes.ExitCodeSMAStartupError);

          return;
        }

        if (SMAExecutableInfo.Instance.IsDev == false)
          await SMAInstaller.Instance.Update().ConfigureAwait(false);
      }
      else
      {
        Shutdown();
      }
    }

    /// <summary>Called when SuperMemo's Element window is loaded</summary>
    private void ElementWdw_OnAvailable()
    {
      Dispatcher.Invoke(() =>
      {
        _splashScreen?.Close();
        _splashScreen = null;
      });

      SMA.Core.SM.UI.ElementWdw.OnAvailable -= ElementWdw_OnAvailable;
    }

    private Task OnSMStartedEventAsync(object sender, SMProcessEventArgs eventArgs)
    {
      SMA.Core.SM.UI.ElementWdw.OnAvailable += ElementWdw_OnAvailable;

      SMAUI.Initialize();

      return Task.CompletedTask;
    }

    private void OnSMStoppedEvent(object sender, SMProcessEventArgs e)
    {
      try
      {
        LogTo.Debug("Cleaning up {Name}", GetType().Name);

        Dispatcher.Invoke(Shutdown);
      }
      finally
      {
        LogTo.Debug("Cleaning up {Name}... Done", GetType().Name);
      }
    }

    /// <summary>Validates the location of SMA on disk</summary>
    /// <param name="error">Error result (if any)</param>
    /// <returns>Whether SMA is in the valid location on disk</returns>
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
