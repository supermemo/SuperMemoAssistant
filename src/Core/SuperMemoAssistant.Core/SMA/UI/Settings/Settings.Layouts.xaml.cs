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
// Created On:   2019/02/27 13:29
// Modified On:  2019/02/27 15:46
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyChanged;
using SuperMemoAssistant.SMA.UI.Settings.Layout;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout.XamlLayouts;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.SMA.UI.Settings
{
  /// <summary>Interaction logic for LayoutSettings.xaml</summary>
  public partial class LayoutSettings : UserControl, INotifyPropertyChanged
  {
    #region Constructors

    public LayoutSettings()
    {
      InitializeComponent();

      Loaded += (o, e) => SelectedLayout = LayoutManager.Layouts.OrderBy(l => l.Name).FirstOrDefault();
    }

    #endregion




    #region Properties & Fields - Public

    public LayoutManager LayoutManager  => LayoutManager.Instance;
    public XamlLayout    SelectedLayout { get; set; }

    public ICommand CloneCommand  => new RelayCommand<XamlLayout>(Clone);
    public ICommand EditCommand   => new RelayCommand<XamlLayout>(Edit, CanEdit);
    public ICommand DeleteCommand => new RelayCommand<XamlLayout>(Delete, CanDelete);

    #endregion




    #region Methods

    private void Clone(XamlLayout layout)
    {
      string layoutName = layout.Name;
      int    i          = 1;

      for (; LayoutManager.LayoutExists(layoutName + i); i++) { }

      var newLayout = new XamlLayout(layout, layoutName + i);

      LayoutManager.AddLayout(newLayout);
      LayoutManager.SaveConfig();
    }

    private void Edit(XamlLayout layout)
    {
      var editor = new LayoutEditorWindow(layout);

      if (editor.ShowDialog() ?? false)
        LayoutManager.SaveConfig();
    }

    private bool CanEdit(XamlLayout layout)
    {
      return layout.IsBuiltIn == false;
    }

    private void Delete(XamlLayout layout)
    {
      LayoutManager.DeleteLayout(layout);
      LayoutManager.SaveConfig();
    }

    private bool CanDelete(XamlLayout layout)
    {
      return layout.IsBuiltIn == false;
    }
    
    [SuppressPropertyChangedWarnings]
    private void OnLayoutSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (CtrlGroup == null || SelectedLayout == null)
        return;

      CtrlGroup.SetXamlLayout(SelectedLayout);
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

  }
}
