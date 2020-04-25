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
// Modified On:  2020/03/13 13:59
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using System.Windows.Input;
using MahApps.Metro.Controls;
using SuperMemoAssistant.Extensions;

// ReSharper disable InvalidXmlDocComment

namespace SuperMemoAssistant.UI
{
  /// <summary>
  ///   The SMA splash screen is displayed after opening a collection in the
  ///   <see cref="CollectionSelectionWindow" /> and before SuperMemo is fully started. This gives
  ///   time to SMA to load the required data (registries, plugins, etc.). SuperMemo Splash Screen
  ///   is cancelled to make the transition smoother, see
  ///   <see cref="SuperMemoAssistant.Hooks.InjectLib.SMInject.ShowWindow_Hooked" />
  /// </summary>
  public partial class SplashScreenWindow : MetroWindow, INotifyPropertyChanged
  {
    #region Constructors

    public SplashScreenWindow()
    {
      SMAVersion = "SMA " + SuperMemoAssistant.SMA.Core.SMAVersion;

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public string SMAVersion { get; set; }

    #endregion




    #region Methods

    private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
        DragMove();
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
