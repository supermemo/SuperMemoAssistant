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
// Modified On:  2020/03/06 14:03
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Anotar.Serilog;
using Forge.Forms;
using NuGet.Protocol.Core.Types;
using PluginManager.Models;
using PluginManager.PackageManager.Models;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.Plugins.Models;
using SuperMemoAssistant.Services.UI.Extensions;
using SuperMemoAssistant.SMA.UI.Settings.Models;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.SMA.UI.ViewModels
{
  public class PluginManagerViewModel : INotifyPropertyChanged
  {
    #region Constants & Statics

    /// <summary><see cref="PluginManagerViewModel" /> Singleton</summary>
    public static PluginManagerViewModel Instance { get; } = new PluginManagerViewModel();

    #endregion




    #region Properties & Fields - Non-Public

    protected bool _ignoreEnabledChanged;

    /// <summary>Commands that use the Package Manager and shouldn't be run at the same time</summary>
    protected List<ICommandEx> PackageManagerCommands { get; }

    /// <summary>Cancels background operations when the he user control is unloaded, or after a delay</summary>
    protected CancellationTokenSource CancellationTokenSource { get; set; }

    /// <summary>Shortcut to <see cref="SMAPluginManager" /></summary>
    protected SMAPluginManager PluginMgr => SMAPluginManager.Instance;

    /// <summary>Global dispatcher to run UI updates on the UI thread</summary>
    protected Dispatcher Dispatcher => Application.Current.Dispatcher;

    #endregion




    #region Constructors

    protected PluginManagerViewModel()
    {
      OperationLogs = new StringBuilder();
      Plugins       = new ObservableCollection<PluginPackage<PluginMetadata>>();

      RefreshCommand = new AsyncRelayCommand<bool>(RefreshOnlinePlugins, CanRefreshPlugin, HandleException);
      InstallCommand = new AsyncRelayCommand<PluginPackage<PluginMetadata>>(InstallPlugin, CanInstallPlugin, HandleException);
      UninstallCommand =
        new AsyncRelayCommand<LocalPluginPackage<PluginMetadata>>(PluginUninstall, CanPluginUninstall, HandleException);
      UpdateCommand        = new AsyncRelayCommand<LocalPluginPackage<PluginMetadata>>(PluginUpdate, CanPluginUpdate, HandleException);
      EnableDisableCommand = new AsyncRelayCommand<PluginInstance>(PluginEnableDisable, null, HandleException);
      PlayPauseCommand     = new AsyncRelayCommand<PluginInstance>(PluginPlayPause, CanPluginPlayPause, HandleException);
      ShowSettingsCommand  = new AsyncRelayCommand<PluginInstance>(PluginShowSettings);
      CancelCommand        = new RelayCommand(CancelTasks);
      BackToTheListCommand = new RelayCommand(BackToTheList);

      PackageManagerCommands = new List<ICommandEx>
      {
        //RefreshCommand,
        InstallCommand,
        UninstallCommand,
        UpdateCommand,
        EnableDisableCommand,
        PlayPauseCommand
      };

      PackageManagerCommands.ForEach(c => c.CanExecuteChanged += PackageManagerCommand_CanExecuteChanged);

      RefreshPlugins(true);
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>
    ///   All available plugins according to the repository API (depending on prerelease
    ///   constraint)
    /// </summary>
    public ObservableCollection<PluginPackage<PluginMetadata>> Plugins { get; private set; }

    /// <summary>All the plugins currently loaded by the Plugin Manager</summary>
    public ReadOnlyObservableCollection<PluginInstance> PluginInstances => PluginMgr.AllPlugins;


    /// <summary>
    ///   Whether any IAsyncCommand's task that requires locking the PluginManager is running.
    ///   Used to prevent commands from running at the same time
    /// </summary>
    public bool IsTaskRunning { get; private set; }

    /// <summary>Refresh available plugins by querying the plugin repository API</summary>
    public AsyncRelayCommand<bool> RefreshCommand { get; }

    /// <summary>Install a new plugin</summary>
    public AsyncRelayCommand<PluginPackage<PluginMetadata>> InstallCommand { get; }

    /// <summary>Uninstall a previously installed plugin</summary>
    public AsyncRelayCommand<LocalPluginPackage<PluginMetadata>> UninstallCommand { get; }

    /// <summary>Update a previously installed plugin</summary>
    public AsyncRelayCommand<LocalPluginPackage<PluginMetadata>> UpdateCommand { get; }

    /// <summary>Enable or disable a loaded plugin</summary>
    public AsyncRelayCommand<PluginInstance> EnableDisableCommand { get; }

    /// <summary>Starts or pauses a loaded plugin</summary>
    public AsyncRelayCommand<PluginInstance> PlayPauseCommand { get; }

    /// <summary>Sends a signal to a running plugin to show its settings dialog</summary>
    public AsyncRelayCommand<PluginInstance> ShowSettingsCommand { get; }

    /// <summary>Cancels the current operation</summary>
    public RelayCommand CancelCommand { get; }

    /// <summary>
    ///   Hides the operation progress display and displays the plugin list. This option is
    ///   available after a fail Install or Uninstall.
    /// </summary>
    public RelayCommand BackToTheListCommand { get; }


    /// <summary>
    ///   Designate the current operation being processed. Helps determining which control to
    ///   display in the UI
    /// </summary>
    public PluginManagerStatus Status { get; private set; } = PluginManagerStatus.Display;

    /// <summary>Error details when a background operation fails</summary>
    public string ErrorMessage { get; private set; }

    /// <summary>Logs of the plugin installation process</summary>
    public StringBuilder OperationLogs { get; }

    /// <summary>Whether to include versions marked as pre-release in the results</summary>
    public bool EnablePrerelease { get; set; } = false; // TODO: Load from config

    #endregion




    #region Methods

    /// <summary>
    ///   Refresh <see cref="Plugins" /> with the latest list of available plugins from the
    ///   online API repository and NuGet repositories. Cached result might be returned if they
    ///   <paramref name="forceRefresh" /> is set to false.
    /// </summary>
    public Task RefreshPlugins(bool forceRefresh)
    {
      if (RefreshCommand.CanExecute(forceRefresh))
        return RefreshCommand.ExecuteAsync(forceRefresh);

      return Task.CompletedTask;
    }

    /// <summary>
    ///   Refresh <see cref="Plugins" /> with the latest list of available plugins from the
    ///   online API repository and NuGet repositories. Cached result might be returned if they
    ///   <paramref name="forceRefresh" /> is set to false.
    /// </summary>
    /// <param name="forceRefresh">Whether to invalidate cache or not</param>
    /// <returns></returns>
    protected virtual async Task RefreshOnlinePlugins(bool forceRefresh)
    {
      try
      {
        Status = PluginManagerStatus.Refresh;

        using (CancellationTokenSource = new CancellationTokenSource(60000))
        {
          var plugins = await PluginMgr.SearchPlugins(forceRefresh: forceRefresh,
                                                      enablePrerelease: EnablePrerelease,
                                                      cancellationToken: CancellationTokenSource.Token)
                                       .ConfigureAwait(true);

          if (plugins == null)
          {
            ErrorMessage =
              "Error: Failed to fetch online plugins. Please check your internet connection and try again. If this issue persists, request assistance from our friendly community.";
            Plugins = null;
            return;
          }

          Plugins      = new ObservableCollection<PluginPackage<PluginMetadata>>(plugins);
          ErrorMessage = null;
        }
      }
      catch (TaskCanceledException)
      {
        if (Plugins == null || Plugins.None())
        {
          Plugins      = null;
          ErrorMessage = "The refresh task has been canceled. Use refresh to display the available plugins";
        }
      }
      finally
      {
        CancellationTokenSource = null;
        Status                  = Plugins == null ? PluginManagerStatus.Error : PluginManagerStatus.Display;
      }
    }

    /// <summary>Checks whether the Refresh command can execute</summary>
    /// <returns>Whether the Refresh command can execute</returns>
    protected virtual bool CanRefreshPlugin(bool _)
    {
      return true; //CanExecuteTask();
    }

    /// <summary>Attempts to install <paramref name="plugin" />.</summary>
    /// <param name="plugin">The plugin selected in the UI</param>
    /// <returns></returns>
    protected virtual async Task InstallPlugin(PluginPackage<PluginMetadata> plugin)
    {
      if (!(plugin is OnlinePluginPackage<PluginMetadata> onlinePlugin))
      {
        LogTo.Error($"Install plugin was called with a type that is not OnlinePluginPackage: {plugin.Serialize()}");
        return;
      }

      bool success = false;

      try
      {
        Status = PluginManagerStatus.Install;

        OperationLogs.Clear();
        PluginMgr.AddLogger(LogOperationOutput);

        using (CancellationTokenSource = new CancellationTokenSource())
          if ((success = await PluginMgr.InstallPluginAsync(onlinePlugin,
                                                            onlinePlugin.SelectedVersion,
                                                            CancellationTokenSource.Token)
                                        .ConfigureAwait(true)) == false)
            return;

        var localPlugin = PluginMgr.AllPlugins.FirstOrDefault(p => p.Package.Id == plugin.Id && p.IsDevelopment == false);

        if (Plugins.Replace(plugin, localPlugin.Package) == false)
          throw new InvalidOperationException("Failed to replace old OnlinePluginPackage with new LocalPluginPackage in Plugins");

        localPlugin.Package.SetOnlineVersions(onlinePlugin);
      }
      catch (InvalidOperationException ex1) when (ex1.InnerException is NuGetProtocolException)
      {
        success = false;
      }
      catch (InvalidOperationException ex2) when (ex2.InnerException is OperationCanceledException)
      {
        success = true;
      }
      finally
      {
        PluginMgr.RemoveLogger(LogOperationOutput);
        CancellationTokenSource = null;

        Status = success ? PluginManagerStatus.Display : PluginManagerStatus.Install;
      }
    }

    /// <summary>Checks whether <paramref name="plugin" /> can be installed</summary>
    /// <param name="plugin">The plugin to install</param>
    /// <returns>Whether <paramref name="plugin" /> can be installed</returns>
    protected virtual bool CanInstallPlugin(PluginPackage<PluginMetadata> plugin)
    {
      return CanExecuteTask() && plugin is OnlinePluginPackage<PluginMetadata>;
    }

    /// <summary>Uninstall <paramref name="pluginPackage" /></summary>
    /// <param name="pluginPackage">The plugin to uninstall</param>
    /// <returns></returns>
    private async Task PluginUninstall(LocalPluginPackage<PluginMetadata> pluginPackage)
    {
      bool success = false;

      try
      {
        Status = PluginManagerStatus.Uninstall;

        OperationLogs.Clear();
        PluginMgr.AddLogger(LogOperationOutput);

        using (CancellationTokenSource = new CancellationTokenSource())
          if ((success = await PluginMgr.UninstallPluginAsync(pluginPackage)
                                        .ConfigureAwait(true)) == false)
            return;

        // Attempt to replace the plugin in the list ; otherwise refresh online plugins
        bool refresh           = true;
        var  onlinePluginsCopy = Plugins;

        if (onlinePluginsCopy != null)
        {
          pluginPackage = onlinePluginsCopy.FirstOrDefault(p => p.Id == pluginPackage.Id) as LocalPluginPackage<PluginMetadata>;

          if (pluginPackage != null && pluginPackage.OnlineVersions != null)
          {
            onlinePluginsCopy.Replace(pluginPackage, new OnlinePluginPackage<PluginMetadata>(pluginPackage));
            refresh = false;
          }
        }

        if (refresh)
          RefreshPlugins(true).RunAsync();
      }
      catch (InvalidOperationException ex2) when (ex2.InnerException is OperationCanceledException)
      {
        success = true;
      }
      finally
      {
        PluginMgr.RemoveLogger(LogOperationOutput);
        CancellationTokenSource = null;

        Status = success ? PluginManagerStatus.Display : PluginManagerStatus.Uninstall;
      }
    }

    /// <summary>
    ///   Checks whether <paramref name="pluginPackage" /> can be uninstalled. Uninstall icon
    ///   is otherwise hidden
    /// </summary>
    /// <param name="pluginPackage">The plugin to uninstall</param>
    /// <returns></returns>
    private bool CanPluginUninstall(LocalPluginPackage<PluginMetadata> pluginPackage)
    {
      return pluginPackage?.Metadata.IsDevelopment == false;
    }

    /// <summary>Updates <paramref name="plugin" /></summary>
    /// <param name="plugin">The plugin to update</param>
    /// <returns></returns>
    private async Task PluginUpdate(LocalPluginPackage<PluginMetadata> plugin)
    {
      bool success = false;

      try
      {
        Status = PluginManagerStatus.Update;

        OperationLogs.Clear();
        PluginMgr.AddLogger(LogOperationOutput);

        using (CancellationTokenSource = new CancellationTokenSource())
          if ((success = await PluginMgr.UpdatePluginAsync(plugin,
                                                           plugin.SelectedVersion,
                                                           EnablePrerelease,
                                                           CancellationTokenSource.Token)
                                        .ConfigureAwait(true)) == false)
            return;

        await PluginMgr.StartPlugin(PluginInstances.FirstOrDefault(pi => pi.Package == plugin)).ConfigureAwait(true);
      }
      catch (InvalidOperationException ex1) when (ex1.InnerException is NuGetProtocolException)
      {
        success = false;
      }
      catch (InvalidOperationException ex2) when (ex2.InnerException is OperationCanceledException)
      {
        success = true;
      }
      finally
      {
        PluginMgr.RemoveLogger(LogOperationOutput);
        CancellationTokenSource = null;

        Status = success ? PluginManagerStatus.Display : PluginManagerStatus.Update;
      }
    }

    /// <summary>
    ///   Checks whether <paramref name="pluginPackage" /> can be updated. Icon is otherwise
    ///   hidden
    /// </summary>
    /// <param name="pluginPackage">The plugin to update</param>
    /// <returns></returns>
    private bool CanPluginUpdate(LocalPluginPackage<PluginMetadata> pluginPackage)
    {
      return pluginPackage?.Metadata.IsDevelopment == false && pluginPackage.HasPendingUpdates;
    }

    /// <summary>Toggle the enabled status of plugin <paramref name="pluginInstance" /></summary>
    /// <param name="pluginInstance">The plugin to enable or disable</param>
    /// <returns></returns>
    private async Task PluginEnableDisable(PluginInstance pluginInstance)
    {
      if (_ignoreEnabledChanged)
      {
        _ignoreEnabledChanged = false;
        return;
      }

      if (pluginInstance.Metadata.Enabled)
        return;

      if (pluginInstance.Status != PluginStatus.Stopped)
      {
        if (await PromptUserConfirmation($"{pluginInstance.Metadata.DisplayName} will be stopped. Continue ?") == false)
        {
          _ignoreEnabledChanged           = true;
          pluginInstance.Metadata.Enabled = true;
          return;
        }

        await PluginMgr.StopPlugin(pluginInstance);
      }

      pluginInstance.Metadata.Enabled = false;
    }

    /// <summary>Starts or stops <paramref name="pluginInstance" /> depending on its current status</summary>
    /// <param name="pluginInstance"></param>
    /// <returns></returns>
    private Task PluginPlayPause(PluginInstance pluginInstance)
    {
      switch (pluginInstance.Status)
      {
        case PluginStatus.Stopped:
          return PluginMgr.StartPlugin(pluginInstance);

        case PluginStatus.Connected:
          return PluginMgr.StopPlugin(pluginInstance);

        default:
          throw new InvalidOperationException($"Invalid plugin status {pluginInstance.Status}");
      }
    }

    /// <summary>
    ///   Checks whether <paramref name="pluginInstance" /> can be either started or stopped.
    ///   Icon is hidden otherwise
    /// </summary>
    /// <param name="pluginInstance"></param>
    /// <returns></returns>
    private bool CanPluginPlayPause(PluginInstance pluginInstance)
    {
      return PluginMgr.CanPluginStartOrPause(pluginInstance);
    }

    /// <summary>
    ///   Relay the signal to <paramref name="pluginInstance" /> that it should display its
    ///   settings dialog
    /// </summary>
    /// <param name="pluginInstance"></param>
    /// <returns></returns>
    private async Task PluginShowSettings(PluginInstance pluginInstance)
    {
      try
      {
        pluginInstance.Plugin.ShowSettings();
      }
      catch (RemotingException)
      {
        await Dispatcher.InvokeAsync(async () => await "Remote plugin unreachable. Try restarting SMA.".ErrorMsgBox());
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    /// <summary>Cancel running tasks using <see cref="CancellationTokenSource" /></summary>
    public void CancelTasks()
    {
      switch (Status)
      {
        case PluginManagerStatus.Install:
          Dispatcher.InvokeAsync(() => OperationLogs.AppendLine("\nCancelling..."));
          break;
      }

      try
      {
        CancellationTokenSource?.Cancel();
      }
      catch (ObjectDisposedException) { }
    }

    /// <summary>Back to the list button. This button is displayed after a fail Install or Uninstall.</summary>
    protected virtual void BackToTheList()
    {
      Status = Plugins?.Any() ?? false
        ? PluginManagerStatus.Display
        : PluginManagerStatus.Error;
    }

    /// <summary>Checks whether any of the command can execute</summary>
    /// <returns>Whether any of the command can execute</returns>
    protected virtual bool CanExecuteTask()
    {
      return IsTaskRunning == false;
    }

    /// <summary>
    ///   Called when any of the <see cref="PackageManagerCommands" /> command is executing.
    ///   Updates the CanExecute status of these commands to avoid running them concurrently.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PackageManagerCommand_CanExecuteChanged(object sender, EventArgs e)
    {
      IsTaskRunning = PackageManagerCommands.Any(c => c.IsExecuting);

      CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>Handle exception throw during ICommand execution</summary>
    /// <param name="ex"></param>
    protected virtual void HandleException(Exception ex)
    {
      LogTo.Error(ex, "Exception occured during a user requested operation on a plugin");
      Dispatcher.Invoke(
        () =>
        {
          var errMsg = $"Operation failed with error: {ex.Message}";
          
          OperationLogs.AppendLine(errMsg);
          errMsg.ErrorMsgBox();
        }
      );
    }

    protected virtual async Task<bool> PromptUserConfirmation(string message)
    {
      return await Dispatcher.InvokeAsync(
        () => Show.Window().For(new Prompt<bool>
        {
          Message = message
        }).Result.Model.Confirmed
      );
    }

    protected void LogOperationOutput(string msg)
    {
      var match = PluginManagerLogAdapter.RE_Anotar.Match(msg);

      if (match.Success)
        msg = match.Groups[match.Groups.Count - 1].Value;

      Dispatcher.Invoke(() =>
      {
        OperationLogs.AppendLine(msg);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OperationLogs)));
      });
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
