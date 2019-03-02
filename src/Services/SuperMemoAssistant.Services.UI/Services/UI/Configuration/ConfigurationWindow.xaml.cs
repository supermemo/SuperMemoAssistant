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
// Created On:   2019/03/01 11:36
// Modified On:  2019/03/01 18:44
// Modified By:  Alexis

#endregion




using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.Services.UI.Configuration
{
  /// <summary>Interaction logic for ConfigurationWindow.xaml</summary>
  public partial class ConfigurationWindow : Window, INotifyPropertyChanged
  {
    #region Constructors

    public ConfigurationWindow(params INotifyPropertyChangedEx[] configModels)
      : this(null, configModels) { }

    public ConfigurationWindow(HotKeyManager hotKeyManager, params INotifyPropertyChangedEx[] configModels)
    {
      InitializeComponent();

      Array.ForEach(configModels, m => Models.Add(m));

      if (hotKeyManager != null)
      {
        Models.Add(hotKeyManager);
        HotKeyManager = hotKeyManager;
      }
    }

    #endregion




    #region Properties & Fields - Public

    public ObservableCollection<object> Models        { get; } = new ObservableCollection<object>();
    public HotKeyManager                HotKeyManager { get; }

    #endregion




    #region Methods Impl

    protected override void OnClosed(EventArgs e)
    {
      base.OnClosed(e);
      
      foreach (var m in Models)
        if (m is INotifyPropertyChangedEx config && config.IsChanged)
        {
          config.IsChanged = false;
          Svc.Configuration.Save(m, m.GetType()).RunAsync();
        }
    }

    #endregion
    



    #region Methods

    private void BtnOk_Click(object          sender,
                             RoutedEventArgs e)
    {
      Close();
    }

    private void Window_KeyDown(object       sender,
                                KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Escape:
        case Key.Enter:
          BtnOk_Click(sender,
                      null);
          break;
      }
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
