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
// Created On:   2019/02/25 22:02
// Modified On:  2019/02/26 15:04
// Modified By:  Alexis

#endregion




using System;
using System.Collections.ObjectModel;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Anotar.Serilog;
using Forge.Forms;
using PluginManager.Models;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.Plugins.Models;
using SuperMemoAssistant.Services.UI.Extensions;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.SMA.UI.Settings
{
  /// <summary>Interaction logic for Settings.xaml</summary>
  public partial class InstalledPluginSettings : UserControl
  {
    #region Properties & Fields - Non-Public

    private bool _ignoreEnabledChanged;

    #endregion




    #region Constructors

    public InstalledPluginSettings()
    {
      DataContext = this;

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public ReadOnlyObservableCollection<PluginInstance> Plugins => SMAPluginManager.Instance.AllPlugins;

    public ICommand PluginShowSettingsCommand => new AsyncRelayCommand<PluginInstance>(PluginShowSettings);

    public ICommand PluginPlayPauseCommand => new AsyncRelayCommand<PluginInstance>(PluginPlayPause, CanPluginPlayPause, HandleException);

    public ICommand PluginUninstallCommand => new AsyncRelayCommand<PluginInstance>(PluginUninstall, CanPluginUninstall, HandleException);

    public ICommand PluginUpdateCommand => new AsyncRelayCommand<PluginInstance>(PluginUpdate, CanPluginUpdate, HandleException);

    public ICommand PluginEnableDisableCommand => new AsyncRelayCommand<PluginInstance>(PluginEnableDisable, null, HandleException);

    #endregion




    #region Methods

    private async Task PluginShowSettings(PluginInstance pluginInstance)
    {
      try
      {
        pluginInstance.Plugin.ShowSettings();
      }
      catch (RemotingException)
      {
        await "Remote plugin unreachable. Try restarting SMA.".ErrorMsgBox();
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }

    private async Task PluginPlayPause(PluginInstance pluginInstance)
    {
      switch (pluginInstance.Status)
      {
        case PluginStatus.Stopped:
          await SMAPluginManager.Instance.StartPlugin(pluginInstance);
          break;

        case PluginStatus.Connected:
          await SMAPluginManager.Instance.StopPlugin(pluginInstance);
          break;

        default:
          throw new InvalidOperationException($"Invalid plugin status {pluginInstance.Status}");
      }
    }

    private bool CanPluginPlayPause(PluginInstance pluginInstance)
    {
      return SMAPluginManager.Instance.CanPluginStartOrPause(pluginInstance);
    }

    private async Task PluginUpdate(PluginInstance pluginInstance)
    {
      await SMAPluginManager.Instance.InstallPlugin(null, null);
    }

    private bool CanPluginUpdate(PluginInstance pluginInstance)
    {
      return pluginInstance?.Metadata.IsDevelopment == false;
    }

    private async Task PluginUninstall(PluginInstance pluginInstance)
    {
      await SMAPluginManager.Instance.UninstallPlugin(pluginInstance);
    }

    private bool CanPluginUninstall(PluginInstance pluginInstance)
    {
      return pluginInstance?.Metadata.IsDevelopment == false;
    }

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

        await SMAPluginManager.Instance.StopPlugin(pluginInstance);
      }

      pluginInstance.Metadata.Enabled = false;
    }

    private void HandleException(Exception ex)
    {
      LogTo.Error(ex, "Exception occured during a user requested operation on a plugin");
      Dispatcher.Invoke(
        () => Show.Window().For(new Alert($"Operation failed: {ex.Message}"))
      );
    }

    private async Task<bool> PromptUserConfirmation(string message)
    {
      return await Dispatcher.InvokeAsync(
        () => Show.Window().For(new Prompt<bool>
        {
          Message = message
        }).Result.Model.Confirmed
      );
    }

    #endregion
  }
}
