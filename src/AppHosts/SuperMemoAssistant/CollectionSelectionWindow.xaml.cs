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
// Modified On:  2019/04/14 21:33
// Modified By:  Alexis

#endregion




using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Forge.Forms;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.Sys.IO;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant
{
  /// <summary>Interaction logic for CollectionSelectionWindow.xaml</summary>
  public partial class CollectionSelectionWindow : MetroWindow
  {
    #region Properties & Fields - Non-Public

    private readonly StartupCfg _config;

    #endregion




    #region Constructors

    public CollectionSelectionWindow()
    {
      _config          = Svc.Configuration.Load<StartupCfg>().Result ?? new StartupCfg();
      SavedCollections = _config.Collections;

      InitializeComponent();

      if (SavedCollections.Count > 0)
        lbCollections.SelectedIndex = 0;

      Loaded += CollectionSelectionWindow_Loaded;
    }

    #endregion




    #region Properties & Fields - Public

    public SMCollection                       Collection       { get; set; }
    public ObservableCollection<SMCollection> SavedCollections { get; }

    public ICommand DeleteCommand => new RelayCommand<SMCollection>(DeleteCollection);

    #endregion




    #region Methods

    private SMCollection CreateCollection(string knoFilePath)
    {
      string filePath = Path.GetDirectoryName(knoFilePath);
      string name     = Path.GetFileNameWithoutExtension(knoFilePath);

      return new SMCollection(name,
                              filePath,
                              DateTime.Now);
    }

    private void DeleteCollection(SMCollection collection)
    {
      if (Forge.Forms.Show.Window().For(new Confirmation("Are you sure ?")).Result.Model.Confirmed)
      {
        SavedCollections.Remove(collection);
        SaveConfig();
      }
    }

    private void SaveConfig()
    {
      Svc.Configuration.Save<StartupCfg>(_config).Wait();
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
        var newCollection = CreateCollection(filePath);
        var duplicate     = _config.Collections.FirstOrDefault(c => c == newCollection);

        if (duplicate != null)
          duplicate.LastOpen = DateTime.Now;

        else
          _config.Collections.Add(newCollection);

        SaveConfig();

        Collection = duplicate ?? newCollection;
        Close();
      }
    }

    private void btnOpen_Click(object          sender,
                               RoutedEventArgs e)
    {
      // Check sm executable exists
      if (new FilePath(_config.SMBinPath).Exists() == false)
      {
        Forge.Forms.Show.Window().For(
          new Alert(
            $"Invalid file path for sm executable file: '{_config.SMBinPath}' could not be found.",
            "Error")
        );
        return;
      }
      
      // Check collection exists
      Collection = (SMCollection)lbCollections.SelectedItem;

      if (File.Exists(Collection.GetKnoFilePath()) == false
        || Directory.Exists(Collection.GetRootDirPath()) == false)
      {
        Forge.Forms.Show.Window().For(new Alert("Collection doesn't exist anymore.", "Error"));

        return;
      }

      // Set last collection usage to now
      Collection.LastOpen = DateTime.Now;

      SaveConfig();

      // Close the collection selection window
      Close();
    }

    private void BtnOptions_Click(object          sender,
                                  RoutedEventArgs e)
    {
      Forge.Forms.Show.Window().For<StartupCfg>(_config).Wait();

      SaveConfig();
    }

    private void Window_KeyDown(object       sender,
                                KeyEventArgs e)
    {
      if (e.Key == Key.Enter && btnOpen.IsEnabled)
        btnOpen_Click(sender,
                      e);

      else if (e.Key == Key.Escape)
        Close();
    }

    private void CollectionSelectionWindow_Loaded(object          sender,
                                                  RoutedEventArgs e)
    {
      lbCollections.SelectFirstItem();
    }

    #endregion
  }
}
