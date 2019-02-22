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
// Created On:   2018/12/30 15:33
// Modified On:  2019/01/18 23:56
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.SMA.UI
{
  /// <summary>Interaction logic for GlobalSettingsWindow.xaml</summary>
  public partial class GlobalSettingsWindow : Window
  {
    #region Constructors

    public GlobalSettingsWindow()
    {
      InitializeComponent();
      
      DataContext = this;

      foreach (var pluginInstance in PluginManager.Instance.GetRunningPlugins())
      {
        var plugin = pluginInstance.Plugin;

        if (plugin.SettingsModels != null)
          foreach (var model in plugin.SettingsModels)
            PluginModelsMap[model] = plugin;
      }

      if (PluginModelsMap.Any())
        tcSettings.SelectedIndex = 0;
    }

    #endregion




    #region Properties & Fields - Public

    public Dictionary<INotifyPropertyChangedEx, ISMAPlugin> PluginModelsMap { get; } =
      new Dictionary<INotifyPropertyChangedEx, ISMAPlugin>();
    public IEnumerable<object> PluginModels => PluginModelsMap.Keys.OrderBy(o => o.ToString());

    #endregion




    #region Methods Impl

    protected override void OnClosed(EventArgs e)
    {
      PluginModelsMap.Clear();

      base.OnClosed(e);
    }

    #endregion




    #region Methods

    private void BtnCancel_Click(object          sender,
                                 RoutedEventArgs e)
    {
      Close();
    }

    private void BtnOk_Click(object          sender,
                             RoutedEventArgs e)
    {
      foreach (var pm in PluginModelsMap)
        if (pm.Key.IsChanged)
          pm.Value.SettingsSaved(pm.Key);

      Close();
    }

    private void Window_KeyDown(object                            sender,
                                System.Windows.Input.KeyEventArgs e)
    {
      switch (e.Key)
      {
        case System.Windows.Input.Key.Enter:
          BtnOk_Click(sender,
                      null);
          break;

        case System.Windows.Input.Key.Escape:
          BtnCancel_Click(sender,
                          null);
          break;
      }
    }

    #endregion
  }
}
