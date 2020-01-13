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
// Created On:   2019/01/01 14:56
// Modified On:  2019/01/01 18:36
// Modified By:  Alexis

#endregion




using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.Services.UI.Configuration.ElementPicker
{
  /// <summary>Interaction logic for ElementPicker.xaml</summary>
  public partial class ElementPicker : Window
  {
    #region Constants & Statics

    public const string ElementPickerAction = "ShowElementPicker";

    #endregion




    #region Properties & Fields - Non-Public

    private bool _suppressRequestBringIntoView;

    #endregion




    #region Constructors

    public ElementPicker()
    {
      DataContext = this;

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public ObservableCollection<IElement> RootElement => new ObservableCollection<IElement> { new ElementWrapper(Svc.SM.Registry.Element.Root) };

    public IElement SelectedElement { get; set; }

    #endregion




    #region Methods

    private void TvElements_MouseDoubleClick(object                                    sender,
                                             System.Windows.Input.MouseButtonEventArgs e)
    {
      BtnOk_Click(sender,
                  null);
    }

    private void BtnCancel_Click(object          sender,
                                 RoutedEventArgs e)
    {
      SelectedElement = null;
      DialogResult    = false;
      Close();
    }

    private void BtnOk_Click(object          sender,
                             RoutedEventArgs e)
    {
      SelectedElement = ((ElementWrapper)TvElements.SelectedItem)?.Original;
      DialogResult    = true;
      Close();
    }

    private void MetroWindow_KeyDown(object                            sender,
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

    private void TvElements_Loaded(object          sender,
                                   RoutedEventArgs e)
    {
      var tviRoot = TvElements.GetTreeViewItem(0);
      tviRoot.IsExpanded = true;
      tviRoot.IsSelected = true;
      tviRoot.Focus();
    }

    private void TreeViewItem_RequestBringIntoView(object                        sender,
                                                   RequestBringIntoViewEventArgs e)
    {
      if (_suppressRequestBringIntoView)
        return;

      var scrollViewer = TvElements.Template.FindName("TreeViewScrollViewer", TvElements) as ScrollViewer;

      if (sender is TreeViewItem tvi && scrollViewer != null)
      {
        _suppressRequestBringIntoView = true;
        e.Handled = true;

        Point topLeftInTreeViewCoordinates = tvi.TransformToAncestor(TvElements).Transform(new Point(0, 0));
        var   treeViewItemTop              = topLeftInTreeViewCoordinates.Y;
        
        // if the item is not visible or too "tall"
        if (treeViewItemTop < 0
          || treeViewItemTop + tvi.ActualHeight > scrollViewer.ViewportHeight
          || tvi.ActualHeight > scrollViewer.ViewportHeight)
        {

          Rect newTargetRect = new Rect(-1000, 0, tvi.ActualWidth + 1000, tvi.ActualHeight);
          tvi.BringIntoView(newTargetRect);
        }
        
        _suppressRequestBringIntoView = false;
      }
    }
    
    private void TreeViewItem_OnSelected(object sender, RoutedEventArgs e)
    {
      ((TreeViewItem)sender).BringIntoView();
      e.Handled = true;
    }

    #endregion
  }
}
