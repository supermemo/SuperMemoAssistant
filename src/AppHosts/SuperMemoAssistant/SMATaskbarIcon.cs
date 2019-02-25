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
// Created On:   2019/02/25 04:36
// Modified On:  2019/02/25 06:47
// Modified By:  Alexis

#endregion




using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Anotar.Serilog;
using Hardcodet.Wpf.TaskbarNotification;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.SMA.UI.Settings;

namespace SuperMemoAssistant
{
  public partial class SMATaskbarIcon : ResourceDictionary
  {
    #region Properties & Fields - Non-Public

    private TaskbarIcon TbIcon { get; set; }

    #endregion




    #region Constructors

    /// <inheritdoc />
    public SMATaskbarIcon()
    {
      InitializeComponent();

      SMA.SMA.Instance.OnSMStartedEvent += OnSMStarted;
    }

    #endregion




    #region Methods

    private Task OnSMStarted(object        sender,
                             SMProcessArgs eventArgs)
    {
      TbIcon.ToolTipText = $"SuperMemoAssistant - {SMA.SMA.Instance.Collection.Name}";
      TbIcon.Visibility  = Visibility.Visible;

      return Task.CompletedTask;
    }

    private void Initialized(object    sender,
                             EventArgs e)
    {
      TbIcon = (TaskbarIcon)sender;

      TbIcon.DataContext = this;
    }

    private void Settings(object          sender,
                          RoutedEventArgs e)
    {
      Application.Current.Dispatcher.Invoke(SettingsWindow.ShowOrActivate);
    }

    private void Exit(object          sender,
                      RoutedEventArgs e)
    {
      TbIcon.Dispose();
      TbIcon = null;

      Environment.Exit(0);
    }

    private void PreviewTrayContextMenuOpen(object          sender,
                                            RoutedEventArgs e)
    {
      var tbIcon = (TaskbarIcon)sender;

      var runningPlugins = PluginManager.Instance.AllPlugins
                                        .Where(p => p.Plugin.HasSettings && p.Status == PluginStatus.Connected)
                                        .OrderBy(p => p.Plugin.Name)
                                        .ToList();

      // ReSharper disable once PossibleNullReferenceException
      while (tbIcon.ContextMenu.Items.Count > 3)
        tbIcon.ContextMenu.Items.RemoveAt(0);

      if (runningPlugins.Any())
        tbIcon.ContextMenu.Items.Insert(0, new Separator());

      foreach (var pluginInstance in runningPlugins)
        try
        {
          var menuItem = new MenuItem
          {
            Header = pluginInstance.Plugin.Name,
          };
          menuItem.Click += (o,
                             _) =>
          {
            try
            {
              pluginInstance.Plugin.OnShowSettings();
            }
            catch (Exception ex)
            {
              LogTo.Error(ex, $"Exception while showing settings for {pluginInstance.Denomination} {pluginInstance.Package.Id}");
            }
          };

          tbIcon.ContextMenu.Items.Insert(0, menuItem);
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, $"Exception while creating Context Menu item for {pluginInstance.Denomination} {pluginInstance.Package.Id}");
        }
    }

    #endregion
  }
}
