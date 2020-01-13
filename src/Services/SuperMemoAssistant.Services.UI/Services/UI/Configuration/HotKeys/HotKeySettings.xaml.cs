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
// Created On:   2019/03/01 01:56
// Modified On:  2019/03/01 15:07
// Modified By:  Alexis

#endregion




using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PropertyChanged;
using SuperMemoAssistant.Services.IO.HotKeys;

namespace SuperMemoAssistant.Services.UI.Configuration.HotKeys
{
  /// <summary>Interaction logic for HotKeySettings.xaml</summary>
  public partial class HotKeySettings : UserControl, INotifyPropertyChanged
  {
    #region Constants & Statics

    public static readonly DependencyProperty HotKeyManagerProperty =
      DependencyProperty.Register(
        "HotKeyManager",
        typeof(HotKeyManager),
        typeof(HotKeySettings),
        new FrameworkPropertyMetadata
        {
          DefaultValue            = null,
          PropertyChangedCallback = OnHotKeyManagerChanged
        }
      );

    #endregion




    #region Constructors

    public HotKeySettings()
    {
      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    [SuppressPropertyChangedWarnings]
    public HotKeyManager HotKeyManager
    {
      set => SetValue(HotKeyManagerProperty, value);
    }

    public ObservableCollection<HotKeyDataBinder> HotKeys { get; set; }

    #endregion




    #region Methods
    
    [SuppressPropertyChangedWarnings]
    private static void OnHotKeyManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var hks = (HotKeySettings)d;
      var hkm = (HotKeyManager)e.NewValue;

      if (hkm == null)
        hks.HotKeys = null;

      else
        hks.HotKeys = new ObservableCollection<HotKeyDataBinder>(
          hkm.HotKeys
             .Select(h => new HotKeyDataBinder(hkm, h))
      );
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
