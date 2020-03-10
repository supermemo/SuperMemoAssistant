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
// Created On:   2019/03/02 18:29
// Modified On:  2019/08/08 11:15
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Forge.Forms;
using MahApps.Metro.Controls;
using PropertyChanged;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlLayouts;
using SuperMemoAssistant.Sys.Threading;

namespace SuperMemoAssistant.SMA.UI.Settings.Layout
{
  /// <summary>Interaction logic for LayoutEditorWindow.xaml</summary>
  public partial class LayoutEditorWindow : MetroWindow, INotifyPropertyChanged
  {
    #region Properties & Fields - Non-Public

    private readonly DelayedTask _parseXamlTask;

    #endregion




    #region Constructors

    public LayoutEditorWindow(XamlLayout xamlLayout)
    {
      OriginalLayout        =  xamlLayout;
      NewLayout             =  new XamlLayout(xamlLayout, autoValidationSuspended: true);
      NewLayout.XamlChanged += OnXamlChanged;

      IsDefault = OriginalLayout.IsDefault;

      _parseXamlTask = new DelayedTask(ParseXaml);

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public XamlLayout NewLayout      { get; }
    public XamlLayout OriginalLayout { get; }
    public bool       IsDefault      { get; set; }
    public string     Errors         { get; set; } = string.Empty;

    #endregion




    #region Methods
    
    [SuppressPropertyChangedWarnings]
    private void OnXamlChanged(XamlLayout xamlLayout, string before, string after)
    {
      _parseXamlTask.Trigger(500);
    }

    private void ParseXaml()
    {
      Dispatcher.Invoke(
        () =>
        {
          XamlLayout.ParseLayout(NewLayout.Xaml, out var parsingEx);

          switch (parsingEx)
          {
            case AggregateException aggEx:
              var messages = aggEx.InnerExceptions.Select(e => e.Message);
              Errors = string.Join("\r\n", messages);
              break;

            case Exception ex:
              Errors = ex.Message;
              break;

            case null:
              Errors = string.Empty;
              break;
          }
        }
      );
    }

    private void BtnCancel_Click(object          sender,
                                 RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }

    private void BtnOk_Click(object          sender,
                             RoutedEventArgs e)
    {
      if (NewLayout.Name != OriginalLayout.Name && LayoutManager.Instance.LayoutExists(NewLayout.Name))
      {
        Forge.Forms.Show.Window().For(new Alert($"'{NewLayout.Name}' is already in use"));
        return;
      }

      NewLayout.ValidateXaml();

      if (NewLayout.IsValid == false)
      {
        var dialog = Forge.Forms.Show
                          .Window()
                          .For(new Confirmation("Invalid XAML markup. Save changes anyway ?", "Warning"));

        if (dialog.Result.Model.Confirmed == false)
          return;
      }

      OriginalLayout.CopyFrom(NewLayout);

      if (IsDefault)
        LayoutManager.Instance.SetDefault(OriginalLayout.Name);

      else if (OriginalLayout.IsDefault)
        LayoutManager.Instance.SetDefault(LayoutManager.GenericLayoutName);

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
    
    [SuppressPropertyChangedWarnings]
    private void OnTabSelectionChanged(object                    sender,
                                       SelectionChangedEventArgs e)
    {
      if (tcLayout.SelectedIndex == 1)
      {
        NewLayout.ValidateXaml();

        CtrlGroup.SetXamlLayout(NewLayout);
      }
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
