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
// Created On:   2019/02/26 23:18
// Modified On:  2019/03/01 21:47
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using PropertyChanged;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlLayouts;

// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlControls
{
  /// <summary>Interaction logic for XamlControlGroup.xaml</summary>
  public partial class XamlControlGroup : UserControl, INotifyPropertyChanged
  {
    #region Constants & Statics

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextContentCountProperty =
      DependencyProperty.Register("TextContentCount", typeof(double), typeof(XamlControlGroup), new PropertyMetadata
      {
        DefaultValue            = 1.0,
        PropertyChangedCallback = OnContentCountChanged
      });

    // Using a DependencyProperty as the backing store for ImageContentCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageContentCountProperty =
      DependencyProperty.Register("ImageContentCount", typeof(double), typeof(XamlControlGroup), new PropertyMetadata
      {
        DefaultValue            = 1.0,
        PropertyChangedCallback = OnContentCountChanged
      });

    // Using a DependencyProperty as the backing store for SoundContentCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SoundContentCountProperty =
      DependencyProperty.Register("SoundContentCount", typeof(double), typeof(XamlControlGroup), new PropertyMetadata
      {
        DefaultValue            = 1.0,
        PropertyChangedCallback = OnContentCountChanged
      });

    #endregion




    #region Properties & Fields - Non-Public

    private readonly bool _displayMode;

    #endregion




    #region Constructors

    /// <inheritdoc />
    public XamlControlGroup()
    {
      _displayMode = true;

      InitializeComponent();
    }

    /// <param name="xamlLayout"></param>
    /// <param name="displayMode"></param>
    /// <inheritdoc />
    public XamlControlGroup(XamlLayout xamlLayout, bool displayMode = false)
    {
      _displayMode = displayMode;

      InitializeComponent();

      SetXamlLayout(xamlLayout);
    }

    #endregion




    #region Properties & Fields - Public

    public double TextContentCount { get => (double)GetValue(TextContentCountProperty); set => SetValue(TextContentCountProperty, value); }


    public double ImageContentCount
    {
      get => (double)GetValue(ImageContentCountProperty);
      set => SetValue(ImageContentCountProperty, value);
    }


    public double SoundContentCount
    {
      get => (double)GetValue(SoundContentCountProperty);
      set => SetValue(SoundContentCountProperty, value);
    }


    public double ScaleX { get; set; } = 1.0;
    public double ScaleY { get; set; } = 1.0;

    #endregion




    #region Methods
    
    [SuppressPropertyChangedWarnings]
    private static void OnContentCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var xcg = (XamlControlGroup)d;

      xcg.GenerateDemoContent();
    }
    
    [SuppressPropertyChangedWarnings]
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (_displayMode)
      {
        ScaleX = Math.Max(e.NewSize.Width * ScaleX / SuperMemoFrameSize.Width, 0.00001);
        ScaleY = Math.Max(e.NewSize.Height * ScaleY / SuperMemoFrameSize.Height, 0.00001);
      }
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
