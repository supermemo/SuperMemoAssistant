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
// Created On:   2020/01/23 08:17
// Modified On:  2020/02/13 21:03
// Modified By:  Alexis

#endregion




using System;
using System.Windows;
using MahApps.Metro.Controls;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins;
using SuperMemoAssistant.Services.IO.Logger;
using SuperMemoAssistant.SMA.Configs;

namespace SuperMemoAssistant.SMA.UI.Settings
{
  /// <summary>Interaction logic for SettingsWindow.xaml</summary>
  public partial class SettingsWindow : MetroWindow
  {
    #region Constants & Statics

    private static SettingsWindow _instance;

    #endregion




    #region Constructors

    public SettingsWindow()
    {
      InitializeComponent();

      DataContext = this;
    }

    #endregion




    #region Properties & Fields - Public

    public CollectionCfg CollectionConfig => Core.SMA.CollectionConfig;
    public LoggerCfg     LoggerConfig     => Core.Logger.Config;

    #endregion




    #region Methods Impl

    protected override void OnClosed(EventArgs e)
    {
      _instance = null;

      if (CollectionConfig.IsChanged)
        Core.SMA.SaveConfig(false);

      if (LoggerConfig.IsChanged)
      {
        Core.Logger.ReloadConfig();
        Core.SharedConfiguration.Save(LoggerConfig);

        SMAPluginManager.Instance.OnLoggerConfigUpdated().RunAsync(); // TODO: Display a notification when updating failed
      }

      base.OnClosed(e);
    }

    #endregion




    #region Methods

    public static void ShowOrActivate()
    {
      if (_instance != null)
      {
        _instance.Activate();
        return;
      }

      _instance = new SettingsWindow();
      _instance.ShowAndActivate();
    }

    private void BtnCancel_Click(object          sender,
                                 RoutedEventArgs e)
    {
      Close();
    }

    private void BtnOk_Click(object          sender,
                             RoutedEventArgs e)
    {
      Close();
    }

    private void Window_KeyDown(object                            sender,
                                System.Windows.Input.KeyEventArgs e)
    {
      switch (e.Key)
      {
        case System.Windows.Input.Key.Enter:
          BtnOk_Click(sender, null);
          break;

        case System.Windows.Input.Key.Escape:
          BtnCancel_Click(sender, null);
          break;
      }
    }

    #endregion
  }
}
