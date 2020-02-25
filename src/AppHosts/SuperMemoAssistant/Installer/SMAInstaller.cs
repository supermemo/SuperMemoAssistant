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
// Modified On:  2020/02/22 17:58
// Modified By:  Alexis

#endregion




using System;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using Nito.AsyncEx;
using Serilog;
using Squirrel;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Models;
using SuperMemoAssistant.Services.IO.Logger;
using SuperMemoAssistant.Sys.Windows.Net;

namespace SuperMemoAssistant.Installer
{
  public sealed class SMAInstaller
  {
    #region Constants & Statics

    private static ILogger      Logger   { get; } = LoggerFactory.CreateSerilog("SuperMemoAssistant.Installer");
    public static  SMAInstaller Instance { get; } = new SMAInstaller();

    public static bool   UpdateEnabled => SuperMemoAssistant.SMA.Core.CoreConfig.Updates.EnableCoreUpdates;
    public static string UpdateUrl     => SuperMemoAssistant.SMA.Core.CoreConfig.Updates.CoreUpdateUrl;

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

    public async Task Update()
    {
      // TODO: Ensure only one Update is running across all instances of SMA (in case SMA is closed during the update process)
      // TODO: Offer manual updates
      // TODO: Add option to wait for user confirmation to update

      if (UpdateEnabled == false)
        return;

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
      }
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
