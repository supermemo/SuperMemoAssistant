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
// Created On:   2019/01/18 19:30
// Modified On:  2019/01/18 20:32
// Modified By:  Alexis

#endregion




using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Forge.Forms;
using SuperMemoAssistant.Interop.SuperMemo.Content.Layout.XamlLayouts;

namespace SuperMemoAssistant.SMA.UI.Layout
{
  /// <summary>Interaction logic for LayoutEditorWindow.xaml</summary>
  public partial class LayoutEditorWindow : Window
  {
    #region Constructors

    public LayoutEditorWindow(XamlLayout xamlLayout)
    {
      XamlLayout = xamlLayout;

      InitializeComponent();
      
      Xaml = XamlLayout.Xaml;
    }

    #endregion




    #region Properties & Fields - Public

    public XamlLayout XamlLayout { get; }

    public string Xaml   { get; set; }
    public object Layout { get; set; }

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
      if (ParseLayout() == null)
      {
        var dialog = Forge.Forms.Show
                          .Dialog()
                          .For(new Confirmation("Invalid XAML markup. Exit without saving ?", "Error"));

        if (dialog.Result.Model.Confirmed == false)
          return;
      }

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
        Layout = ParseLayout() ?? CreateErrorControl();
    }

    private DependencyObject ParseLayout()
    {
      var context = new ParserContext { XamlTypeMapper = new XamlTypeMapper(new string[] { }) };

      var @namespace   = nameof(Interop.SuperMemo.Content.Layout.XamlControls);
      var assemblyName = "SuperMemoAssistant.Interop";

      context.XmlnsDictionary.Add("sma", $"clr-namespace:{@namespace}");
      // ReSharper disable once AssignNullToNotNullAttribute
      context.XamlTypeMapper.AddMappingProcessingInstruction($"clr-namespace:{@namespace}", @namespace, assemblyName);

      return XamlReader.Parse(Xaml, context) as DependencyObject;
    }

    private DependencyObject CreateErrorControl()
    {
      return new Label
      {
        VerticalAlignment   = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Content             = "Invalid XAML markup."
      };
    }

    #endregion
  }
}
