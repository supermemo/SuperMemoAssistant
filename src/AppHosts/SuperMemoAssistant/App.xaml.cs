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
// Modified On:  2020/02/03 16:41
// Modified By:  Alexis

#endregion




using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using CommandLine;
using Hardcodet.Wpf.TaskbarNotification;
using SuperMemoAssistant.Exceptions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.PluginHost;
using SuperMemoAssistant.Services.UI.Extensions;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.SMA.Utils;
using SuperMemoAssistant.Sys.IO;
using SuperMemoAssistant.Sys.IO.Devices;
using SuperMemoFinderUtil = SuperMemoAssistant.SMA.Utils.SuperMemoFinder;

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
      if (Parser.Default.ParseArguments<SMAParameters>(e.Args) is Parsed<SMAParameters> parsed)
        await LoadApp(parsed.Value);

      else
        Shutdown(HostConst.ExitParameters);
    }

    private async Task LoadApp(SMAParameters args)
    {
      _taskbarIcon = (TaskbarIcon)FindResource("TbIcon");

      //
      // Make sure assemblies are available, and SMA is installed in "%AppData%\SuperMemoAssistant"
      if (CheckAssemblies(out var errMsg) == false || CheckSMALocation(out errMsg) == false)
      {
        LogTo.Warning(errMsg);
        await errMsg.ErrorMsgBox();

        Shutdown(1);
        return;
      }

      //
      // Load system configs
      if (await LoadConfigs(out var nativeDataCfg, out var startupCfg) == false)
        return;

      //
      // Make sure SuperMemo exe path is correct. Prompt user to input the path otherwise.
      if (ShouldFindSuperMemo(startupCfg, nativeDataCfg))
      {
        var smFinder = new SuperMemoFinder(nativeDataCfg, startupCfg);
        smFinder.ShowDialog();

        if (smFinder.DialogResult == null || smFinder.DialogResult == false)
        {
          LogTo.Warning("No valid SM executable file path defined. SMA cannot run.");

          Shutdown(1);
          return;
        }
      }

      //
      // (Optional) Start the debug Key logger (logs key strokes with modifiers, e.g. ctrl, alt, ..)
      if (args.KeyLogger)
        SMA.Core.KeyboardHotKey.MainCallback = LogHotKeys;

      //
      // Determine which collection to open
      SMCollection smCollection = null;
      var          selectionWdw = new CollectionSelectionWindow(startupCfg);

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
        SMA.Core.SMA.OnSMStoppedEvent += Instance_OnSMStoppedEvent;

        if (await SMA.Core.SMA.Start(nativeDataCfg, startupCfg, smCollection).ConfigureAwait(true) == false)
        {
          await $"SMA failed to start. Please check the logs in '{SMAFileSystem.LogDir.FullPath}' for details.".ErrorMsgBox();
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

    private bool ShouldFindSuperMemo(StartupCfg startupCfg, NativeDataCfg nativeDataCfg)
    {
      return string.IsNullOrWhiteSpace(startupCfg.SMBinPath)
        || SuperMemoFinderUtil.CheckSuperMemoExecutable(
          nativeDataCfg,
          startupCfg.SMBinPath,
          out _,
          out _) == false;
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

    private Task<bool> LoadConfigs(out NativeDataCfg nativeDataCfg, out StartupCfg startupCfg)
    {
      nativeDataCfg = LoadNativeDataConfig().Result;
      startupCfg    = LoadStartupConfig().Result;

      if (nativeDataCfg == null || startupCfg == null)
      {
        Shutdown(1);
        return Task.FromResult(false);
      }

      return Task.FromResult(true);
    }

    private async Task<StartupCfg> LoadStartupConfig()
    {
      try
      {
        return await SMA.Core.Configuration.Load<StartupCfg>()
                        .ConfigureAwait(false) ?? new StartupCfg();
      }
      catch (SMAException)
      {
        await "Failed to open StartupCfg.json. Make sure file is unlocked and try again.".ErrorMsgBox();

        return null;
      }
    }

    private async Task<NativeDataCfg> LoadNativeDataConfig()
    {
      try
      {
        return await SMA.Core.Configuration.Load<NativeDataCfg>(SMAFileSystem.AppRootDir)
                        .ConfigureAwait(false);
      }
      catch (SMAException)
      {
        await "Failed to load native data config file.".ErrorMsgBox();

        return null;
      }
    }

    #endregion
  }
}
