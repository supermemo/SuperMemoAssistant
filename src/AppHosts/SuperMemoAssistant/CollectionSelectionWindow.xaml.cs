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
// Created On:   2018/11/22 15:10
// Modified On:  2018/11/22 18:13
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SuperMemoAssistant.Interop.SuperMemo.Core;

namespace SuperMemoAssistant
{
  /// <summary>Interaction logic for CollectionSelectionWindow.xaml</summary>
  public partial class CollectionSelectionWindow : Window
  {
    #region Constructors

    public CollectionSelectionWindow()
    {
      InitializeComponent();

      LoadCollections();
    }

    #endregion




    #region Properties & Fields - Public

    public SMCollection                       Collection       { get; set; }
    public ObservableCollection<SMCollection> SavedCollections { get; set; }

    #endregion




    #region Methods

    private void LoadCollections()
    {
      SavedCollections = new ObservableCollection<SMCollection>();

      var savedKnos = GetSavedKnos();

      foreach (var collection in savedKnos.Select(KnoToCollection))
        SavedCollections.Add(collection);

      lbCollections.ItemsSource = SavedCollections;

      if (SavedCollections.Count > 0)
        lbCollections.SelectedIndex = 0;
    }

    private void btnBrowse_Click(object          sender,
                                 RoutedEventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog
      {
        DefaultExt = ".kno",
        Filter     = "SM Collection (*.kno)|*.kno|All files (*.*)|*.*"
      };

      var filePath = dlg.ShowDialog().GetValueOrDefault(false)
        ? dlg.FileName
        : null;

      if (filePath != null)
      {
        Collection = KnoToCollection(filePath);

        AddNewCollectionToConfig(filePath);

        Close();
      }
    }

    private void btnOpen_Click(object          sender,
                               RoutedEventArgs e)
    {
      Collection = (SMCollection)lbCollections.SelectedItem;

      ReorderCollectionToFront(lbCollections.SelectedIndex);

      Close();
    }

    private void ReorderCollectionToFront(int index)
    {
      if (Properties.Settings.Default.SavedCollections == null
        || Properties.Settings.Default.SavedCollections.Count <= index)
        return;

      var savedCollections = Properties.Settings.Default.SavedCollections;
      var col              = savedCollections[index];

      savedCollections.RemoveAt(index);
      savedCollections.Insert(0,
                              col);

      Properties.Settings.Default.SavedCollections = savedCollections;
      Properties.Settings.Default.Save();
    }

    private void AddNewCollectionToConfig(string knoFilePath)
    {
      var savedCollections = Properties.Settings.Default.SavedCollections ?? new System.Collections.Specialized.StringCollection();

      savedCollections.Insert(0,
                              knoFilePath);

      Properties.Settings.Default.SavedCollections = savedCollections;
      Properties.Settings.Default.Save();
    }

    private SMCollection KnoToCollection(string knoFilePath)
    {
      string filePath = Path.GetDirectoryName(knoFilePath);
      string name     = Path.GetFileNameWithoutExtension(knoFilePath);

      return new SMCollection(name,
                              filePath);
    }

    private List<string> GetSavedKnos()
    {
      return Properties.Settings.Default.SavedCollections?.Cast<string>().ToList()
        ?? new List<string>();
    }

    #endregion

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Enter && btnOpen.IsEnabled)
        btnOpen_Click(sender,
                      e);
    }
  }
}
