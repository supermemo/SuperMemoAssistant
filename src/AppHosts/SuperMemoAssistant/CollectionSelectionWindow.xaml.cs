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
// Created On:   2020/01/13 16:38
// Modified On:  2020/01/13 17:03
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
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.Sys.IO;
using SuperMemoAssistant.Sys.Windows.Input;
using SuperMemoAssistant.Interop;
using Anotar.Serilog;
using System.Diagnostics;
using System.Windows.Navigation;

namespace SuperMemoAssistant
{
  /// <summary>Interaction logic for CollectionSelectionWindow.xaml</summary>
  public partial class CollectionSelectionWindow : MetroWindow
  {
    #region Properties & Fields - Non-Public

    private readonly StartupCfg _startupCfg;

    #endregion




    #region Constructors

    public CollectionSelectionWindow(StartupCfg startupCfg)
    {
      _startupCfg = startupCfg;
      SavedCollections = startupCfg.Collections;

      InitializeComponent();

      if (SavedCollections.Count > 0)
        lbCollections.SelectedIndex = 0;

      Loaded += CollectionSelectionWindow_Loaded;
      Loaded += ShowQuoteOfTheDay;
    }

    #endregion




    #region Properties & Fields - Public

    public SMCollection                       Collection       { get; set; }
    public ObservableCollection<SMCollection> SavedCollections { get; }

    public ICommand DeleteCommand => new RelayCommand<SMCollection>(DeleteCollection);

    #endregion




    #region Methods

    public bool ValidateSuperMemoPath()
    {
      if (new FilePath(_startupCfg.SMBinPath).Exists() == false)
      {
        Forge.Forms.Show.Window().For(
          new Alert(
            $"Invalid file path for sm executable file: '{_startupCfg.SMBinPath}' could not be found.",
            "Error")
        );
        return false;
      }

      return true;
    }

    public bool ValidateCollection(SMCollection collection)
    {
      var knoFilePath = new FilePath(collection.GetKnoFilePath());

      if (knoFilePath.Exists() == false
        || Directory.Exists(collection.GetRootDirPath()) == false)
      {
        Forge.Forms.Show.Window().For(new Alert("Collection doesn't exist anymore.", "Error"));

        return false;
      }

      // Check whether collection is locked
      if (knoFilePath.IsLocked())
      {
        Forge.Forms.Show.Window().For(new Alert("Collection is locked. Is SuperMemo already running ?", "Error"));

        return false;
      }

      return true;
    }

    private SMCollection CreateCollection(string knoFilePath)
    {
      string filePath = Path.GetDirectoryName(knoFilePath);
      string name     = Path.GetFileNameWithoutExtension(knoFilePath);

      return new SMCollection(name, filePath, DateTime.Now);
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
      SuperMemoAssistant.SMA.Core.Configuration.Save<StartupCfg>(_startupCfg).Wait();
    }

    private void OpenSelectedCollection()
    {
      // Check whether sm executable exists
      if (ValidateSuperMemoPath() == false)
        return;

      // Check collection exists
      var collection = (SMCollection)lbCollections.SelectedItem;

      if (ValidateCollection(collection) == false)
        return;

      // We're a go
      Collection = collection;

      // Set last collection usage to now
      Collection.LastOpen = DateTime.Now;

      SaveConfig();

      // Close the collection selection window
      Close();
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
        var duplicate     = _startupCfg.Collections.FirstOrDefault(c => c == newCollection);

        if (duplicate != null)
          duplicate.LastOpen = DateTime.Now;

        else
          _startupCfg.Collections.Add(newCollection);

        SaveConfig();

        Collection = duplicate ?? newCollection;
        Close();
      }
    }

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      OpenSelectedCollection();
    }

    private void btnOpen_Click(object          sender,
                               RoutedEventArgs e)
    {
      OpenSelectedCollection();
    }

    private void BtnOptions_Click(object          sender,
                                  RoutedEventArgs e)
    {
      Forge.Forms.Show.Window().For<StartupCfg>(_startupCfg).Wait();

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

    private void TitleLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
      e.Handled = true;
    }

    private void ShowQuoteOfTheDay(object sender, RoutedEventArgs e)
    {

      var QuoteFile = SMAFileSystem.AppRootDir.CombineFile("quotes.csv");

      if (QuoteFile.Exists())
      {
        try
        {
          var Lines = File.ReadAllLines(QuoteFile.FullPath);

          // Line 1 is .tsv heading
          if (Lines.Length <= 1)
          {
            return;
          }

          var RandInt = new Random();
          var RandomLineNumber = RandInt.Next(1, Lines.Length - 1);
          var QuoteLine = Lines[RandomLineNumber];
          var SplitQuoteLine = QuoteLine.Split('\t');
          
          // Tab separated file
          // Field 0: Quote
          // Field 1: Author
          // Field 2: Url
          // Field 3: Title
          if (!SplitQuoteLine[0].EndsWith(".")
              && !SplitQuoteLine[0].EndsWith("!")
              && !SplitQuoteLine[0].EndsWith("?"))
          {
            SplitQuoteLine[0] += ".";
          }

          QuoteBodyTextBlock.Text = "\"" + SplitQuoteLine[0] + "\"";
          QuoteAuthorTextBlock.Text = SplitQuoteLine[1];
          TitleHyperlink.NavigateUri = new Uri(SplitQuoteLine[2]);
          QuoteTitleTextBlock.Text = SplitQuoteLine[3];
        }
        catch (IOException ex)
        {
          LogTo.Warning(ex, $"IOException when trying to open {QuoteFile.FullPath}");
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, $"Exception caught while opening file {QuoteFile.FullPath}");
        }
      }
    }
    #endregion
  }
}
