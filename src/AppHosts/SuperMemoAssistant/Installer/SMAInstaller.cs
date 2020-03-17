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
// Modified On:  2020/03/15 17:29
// Modified By:  Alexis

#endregion




using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Anotar.Serilog;
using Microsoft.Toolkit.Uwp.Notifications;
using Nito.AsyncEx;
using Serilog;
using Squirrel;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Models;
using SuperMemoAssistant.Services.IO.Logger;
using SuperMemoAssistant.Sys.Windows;
using SuperMemoAssistant.Sys.Windows.Net;

namespace SuperMemoAssistant.Installer
{
  /// <summary>
  /// Handles the updating process for SMA, and events sent from the installer itself (e.g. OnInstalled, OnUpdated, etc.)
  /// </summary>
  public sealed class SMAInstaller
  {
    #region Constants & Statics

    private static ILogger      Logger   { get; } = LoggerFactory.CreateSerilog("SuperMemoAssistant.Installer");
    public static  SMAInstaller Instance { get; } = new SMAInstaller();

    public static bool   UpdateEnabled => SMA.Core.CoreConfig.Updates.EnableCoreUpdates;
    public static string UpdateUrl     => SMA.Core.CoreConfig?.Updates.CoreUpdateUrl;

    #endregion




    #region Properties & Fields - Non-Public

    private readonly AsyncSemaphore _semaphore = new AsyncSemaphore(1);

    #endregion




    #region Constructors

    private SMAInstaller() { }

    #endregion




    #region Properties & Fields - Public

    public SMAUpdateState State       { get; private set; } = SMAUpdateState.Idle;
    public int            ProgressPct { get; private set; }

    #endregion




    #region Methods

    private static UpdateManager CreateUpdateMgr() => new UpdateManager(UpdateUrl);

    /// <summary>Handles event notifications from the installer (e.g. installed, updated, etc.)</summary>
    /// <param name="parameters">The command line parameters</param>
    /// <param name="firstRun">Whether SMA is run for the first time</param>
    /// <returns>Whether an event has been handled (in which case the program should exit)</returns>
    public static bool HandleEvent(SMAParameters parameters, out bool firstRun)
    {
      firstRun = parameters.SquirrelFirstRun;

      if (parameters.SquirrelInstalled != null)
        using (var mgr = CreateUpdateMgr())
        {
          Logger.Information($"SuperMemo Assistant version {parameters.SquirrelInstalled} installed. Creating shortcuts.");
          mgr.CreateShortcutForThisExe();

          return true;
        }

      if (parameters.SquirrelUninstalled != null)
        using (var mgr = CreateUpdateMgr())
        {
          Logger.Information($"SuperMemo Assistant version {parameters.SquirrelUninstalled} uninstalled. Removing shortcuts.");
          mgr.RemoveShortcutForThisExe();

          return true;
        }

      if (parameters.SquirrelUpdated != null) // TODO: Remove previous versions
      {
        Logger.Information($"SuperMemo Assistant has been updated to version {parameters.SquirrelUpdated}.");
        return true;
      }

      if (parameters.SquirrelObsoleted != null)
        //Logger.Information($"SuperMemo Assistant version {parameters.SquirrelObsoleted} has been made obsolete by a newer installed version.");
        return true;

      return firstRun;
    }

    /// <summary>Run the SMA update process</summary>
    /// <returns></returns>
    public async Task Update()
    {
      // TODO: Ensure only one Update is running across all instances of SMA (in case SMA is closed during the update process)
      // TODO: Offer manual updates
      // TODO: Add option to wait for user confirmation to update

      if (UpdateEnabled == false)
        return;

      ReleaseEntry updateVersion = null;

      try
      {
        if (Wininet.HasNetworking() == false)
          return;

        CancellationTokenSource cts = new CancellationTokenSource(0);
        cts.Cancel();

        using (await _semaphore.LockAsync(cts.Token))
        using (var updateMgr = CreateUpdateMgr())
        {
          State = SMAUpdateState.Fetching;

          var updateInfo = await updateMgr.CheckForUpdate(false, progress => ProgressPct = progress);

          if (updateInfo?.ReleasesToApply == null)
          {
            State = SMAUpdateState.Error;
            return;
          }

          if (updateInfo.ReleasesToApply.None())
          {
            State = SMAUpdateState.UpToDate;
            return;
          }

          updateVersion = updateInfo.FutureReleaseEntry;

          State = SMAUpdateState.Downloading;

          await updateMgr.DownloadReleases(updateInfo.ReleasesToApply, progress => ProgressPct = progress);

          State = SMAUpdateState.Applying;

          await updateMgr.ApplyReleases(updateInfo, progress => ProgressPct = progress);

          State = SMAUpdateState.CreatingUninstaller;

          await updateMgr.CreateUninstallerRegistryEntry();

          State = SMAUpdateState.Updated;
        }
      }
      catch (TaskCanceledException) { }
      catch (Exception ex) // TODO: Update Squirrel UpdateManager to send sub-classed Exceptions
      {
        LogTo.Warning(ex, $"An exception was caught while {State.Name().ToLower()} update");
        State = SMAUpdateState.Error;
      }
      finally
      {
        _semaphore.Release();

        if (updateVersion != null)
          NotifyUpdateResult(updateVersion);
      }
    }

    /// <summary>Sends a Windows desktop notification toast about the success or failure of the update</summary>
    /// <param name="updateVersion">The final release version</param>
    private void NotifyUpdateResult(ReleaseEntry updateVersion)
    {
      var msg = State == SMAUpdateState.Updated
        ? $"SMA has been updated to version {updateVersion.Version}. Restart SMA to use the new version."
        : $"An error occured while updating SMA to version {updateVersion.Version}. Check the logs for more information.";

      ToastContent toastContent = new ToastContent
      {
        Visual = new ToastVisual
        {
          BindingGeneric = new ToastBindingGeneric
          {
            Children =
            {
              new AdaptiveText
              {
                Text = msg
              }
            }
          }
        }
      };

      if (State == SMAUpdateState.Error)
        toastContent.Actions = new ToastActionsCustom
        {
          Buttons =
          {
            new ToastButton("Open the logs folder", SMAFileSystem.LogDir.FullPathWin)
            {
              ActivationType = ToastActivationType.Protocol
            }
          }
        };

      var doc = new XmlDocument();
      doc.LoadXml(toastContent.GetContent());

      // And create the toast notification
      var toast = new ToastNotification(doc);

      // And then show it
      DesktopNotificationManager.CreateToastNotifier().Show(toast);
    }

    #endregion
  }

  public enum SMAUpdateState
  {
    Idle,
    Error,
    UpToDate,
    Fetching,
    Downloading,
    Applying,
    CreatingUninstaller,
    Updated,
  }
}
