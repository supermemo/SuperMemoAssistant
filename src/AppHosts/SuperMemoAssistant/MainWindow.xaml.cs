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
// Created On:   2018/05/08 15:19
// Modified On:  2018/06/10 09:37
// Modified By:  Alexis

#endregion




using System;
using System.Windows;
using Patagames.Pdf.Net;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Components.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo;

namespace SuperMemoAssistant
{
  /// <summary>Interaction logic for MainWindow.xaml</summary>
  public partial class MainWindow : Window
  {
    #region Constructors

    public MainWindow()
    {
      InitializeComponent();
      //Visibility = Visibility.Hidden;

      var smCol = new SMCollection("SMATest", SMConst.CollectionPath);
      bool ret = SMA.Instance.Start(smCol);

      //TestPdf();
    }

    #endregion




    #region Methods Impl

    protected override void OnClosed(EventArgs e)
    {
      //SMA.Instance.Dispose();

      base.OnClosed(e);
    }

    #endregion




    #region Methods

    private void TestPdf()
    {
      PdfCommon.Initialize();

      IPDFViewer.LoadDocument("D:\\Temp\\test2.pdf");
    }

    private void Test_Click(object          sender,
                            RoutedEventArgs e)
    {
      //bool res1 = SMA.Instance.UI.ElementWindow.GoToElement(1);
      //bool res2 = SMA.Instance.UI.ElementWindow.PasteArticle(1,
      //                                           "This is <b>HTML</b>.");

      //System.Diagnostics.Debug.WriteLine($"{res1} {res2}");

      var ctrl = SMA.Instance.UI.ElementWindow.ControlGroup[0];

      switch (ctrl.Type)
      {
        case ComponentType.Html:
          var htmlCtrl = ctrl as IControlWeb;
          
          System.Diagnostics.Debug.WriteLine(htmlCtrl.Text);
          break;

        case ComponentType.Text:
        case ComponentType.Rtf:
          var textCtrl = ctrl as IControlTextBase;

          System.Diagnostics.Debug.WriteLine(textCtrl.Text);
          break;
      }

      //SMA.Instance.UI.ElementWindow.Done();

      //IPDFViewer.ToImg();


      //SMA.Instance.MainBarWindow.OpenMenu(
      //  m => m.EditMenu.MenusMenu.Elements()
      //);


      //UIAutomation.Devices.Messaging.BackgroundMouseClick(
      //  SMA.Instance.MainBarWindow.GetAutomationElement().Properties.NativeWindowHandle.ValueOrDefault,
      //  IO.Devices.VKey.KEY_LBUTTON,
      //  //5, 5
      //  33, 69
      //);
      //SMA.Instance.MainBarWindow.GetAutomationElement().Focus();

      //var editMenu = SMA.Instance.MainBarWindow.MainMenu.EditMenu;//.MenusMenu;

      //Messaging.PostSysKeys(
      //  SMA.Instance.MainBarWindow.GetAutomationElement().Properties.NativeWindowHandle.ValueOrDefault,
      //  new Keys(false, true, false, Key.F)
      //);

      //UIAutomation.Devices.Messaging.SendKeys(
      //  SMA.Instance.ElementWindow.HTMLHwnd,
      //  new Keys(false, true, false, Key.H, Key.E, Key.L, Key.L, Key.O)
      //);
    }

    #endregion
  }
}
