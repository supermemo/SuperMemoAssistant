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
// Modified On:  2019/02/27 13:11
// Modified By:  Alexis

#endregion




using System.Windows;
using System.Windows.Controls;
using Forge.Forms;
using MahApps.Metro.Controls;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Content.Layout.XamlLayouts;

namespace SuperMemoAssistant.SMA.UI.Layout
{
  /// <summary>Interaction logic for LayoutEditorWindow.xaml</summary>
  public partial class LayoutEditorWindow : MetroWindow
  {
    #region Constructors

    public LayoutEditorWindow(XamlLayout xamlLayout)
    {
      OriginalLayout = xamlLayout;
      NewLayout      = new XamlLayout(xamlLayout, autoValidationSuspended: true);

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public XamlLayout NewLayout      { get; }
    public XamlLayout OriginalLayout { get; }

    #endregion




    #region Methods

    private void BtnCancel_Click(object          sender,
                                 RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }

    private void BtnOk_Click(object          sender,
                             RoutedEventArgs e)
    {
      NewLayout.ValidateXaml();

      if (NewLayout.IsValid == false)
      {
        var dialog = Forge.Forms.Show
                          .Dialog()
                          .For(new Confirmation("Invalid XAML markup. Save anyway ?", "Warning"));

        if (dialog.Result.Model.Confirmed == false)
          return;
      }

      OriginalLayout.CopyFrom(NewLayout);

      DialogResult = true;
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

    private void TcLayout_SelectionChanged(object                    sender,
                                           SelectionChangedEventArgs e)
    {
      if (tcLayout.SelectedIndex == 1)
      {
        NewLayout.ValidateXaml();

        CtrlGroup.SetXamlLayout(NewLayout);
      }
    }

    #endregion
  }
}
