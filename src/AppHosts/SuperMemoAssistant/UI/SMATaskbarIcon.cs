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
// Modified On:  2020/02/21 22:59
// Modified By:  Alexis

#endregion




using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Anotar.Serilog;
using Hardcodet.Wpf.TaskbarNotification;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.SMA.UI.Settings;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Elements.Types;
using Task = System.Threading.Tasks.Task;

// ReSharper disable UnusedParameter.Local

namespace SuperMemoAssistant.UI
{
  public partial class SMATaskbarIcon : ResourceDictionary
  {
    #region Constants & Statics

    private const string MenuPluginTag = "PluginItem";

    #endregion




    #region Properties & Fields - Non-Public

    private TaskbarIcon TbIcon { get; set; }

    #endregion




    #region Constructors

    /// <inheritdoc />
    public SMATaskbarIcon()
    {
      InitializeComponent();

      SMA.Core.SMA.OnSMStartedEvent += OnSMStarted;
    }

    #endregion




    #region Methods

    private async Task OnSMStarted(object        sender,
                                   SMProcessArgs eventArgs)
    {
      await Application.Current.Dispatcher.InvokeAsync(() =>
      {
        TbIcon.ToolTipText = $"SuperMemoAssistant - {SMA.Core.SM.Collection.Name}";
        TbIcon.Visibility  = Visibility.Visible;
      });
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

#if DEBUG
    private void Test(object          sender,
                      RoutedEventArgs e)
    {
      // ReSharper disable once UnusedVariable
      var curEl = (ElementBase)SMA.Core.SM.UI.ElementWdw.CurrentElement;
    }
#endif

    private void PreviewTrayContextMenuOpen(object          sender,
                                            RoutedEventArgs e)
    {
      var tbIcon = (TaskbarIcon)sender;

      var runningPlugins = SMAPluginManager.Instance.AllPlugins
                                        .Where(p => p.HasSettings)
                                        .OrderBy(p => p.Metadata.DisplayName)
                                        .ToList();

      // ReSharper disable once PossibleNullReferenceException
      while (tbIcon.ContextMenu.Items.Count > 0 && MenuPluginTag.Equals(((Control)tbIcon.ContextMenu.Items.GetItemAt(0)).Tag))
        tbIcon.ContextMenu.Items.RemoveAt(0);

      if (runningPlugins.Any())
        tbIcon.ContextMenu.Items.Insert(0, new Separator() { Tag = MenuPluginTag });

      foreach (var pluginInstance in runningPlugins)
        try
        {
          var menuItem = new MenuItem
          {
            Header = pluginInstance.Metadata.DisplayName,
            Tag    = MenuPluginTag
          };
          menuItem.Click += (o, _) =>
          {
            try
            {
              pluginInstance.Plugin.ShowSettings();
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
