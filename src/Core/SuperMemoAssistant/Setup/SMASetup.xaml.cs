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
// Modified On:  2020/03/11 00:55
// Modified By:  Alexis

#endregion




using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Anotar.Serilog;
using MahApps.Metro.Controls;
using SuperMemoAssistant.Services.UI.Extensions;
using SuperMemoAssistant.Setup.Screens;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.Setup
{
  /// <summary>
  ///   Handles the Setup logic for SMA. Setup steps are displayed in this window under the
  ///   form of "screens", see <see cref="SMASetupScreenBase" />. If no Setup is required, this
  ///   window is never displayed.
  /// </summary>
  public partial class SMASetup : MetroWindow, INotifyPropertyChanged
  {
    #region Constructors

    protected SMASetup(NativeDataCfg nativeDataCfg, CoreCfg startupCfg)
    {
      NextCommand = new RelayCommand(ShowNextScreen /*, CanNextExecute*/);
      BackCommand = new RelayCommand(ShowPreviousScreen);

      Screens = new ObservableCollection<SMASetupScreenBase>
      {
        new TermsOfLicense(startupCfg),
        new SuperMemoFinder(nativeDataCfg, startupCfg),
        new ImportCollections(startupCfg),
        new PluginSetup(),
      };

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>All the setup screens</summary>
    public ObservableCollection<SMASetupScreenBase> Screens { get; }

    /// <summary>The currently displayed setup screen</summary>
    public SMASetupScreenBase CurrentScreen { get; private set; }

    /// <summary>The "Next" button's command</summary>
    public ICommand NextCommand { get; }

    /// <summary>The "Back" button's command</summary>
    public ICommand BackCommand { get; }

    #endregion




    #region Methods

    public static bool Run(NativeDataCfg nativeDataCfg, CoreCfg startupCfg)
    {
      var setup = new SMASetup(nativeDataCfg, startupCfg);

      if (setup.IsSetupRequired() == false)
        return true;

      setup.ShowDialog();

      return setup.DialogResult ?? false;
    }

    private bool IsSetupRequired()
    {
      return Screens.Any(s => s.IsSetup == false);
    }

    private void ShowNextScreen()
    {
      var nextScreen = FindNextScreen();

      if (nextScreen != null)
      {
        ShowScreen(nextScreen);
        return;
      }

      DialogResult = true;
      Close();

      ShowScreen(null);
    }

    private void ShowPreviousScreen()
    {
      int screenIdx = Screens.IndexOf(CurrentScreen);

      if (screenIdx < 1)
      {
        LogTo.Error("ShowPreviousScreen called with invalid CurrentScreen: Screens.IndexOf(CurrentScreen) < 1");
        return;
      }

      ShowScreen(Screens.ElementAt(screenIdx - 1));
    }

    private void ShowScreen(SMASetupScreenBase screen)
    {
      try
      {
        CurrentScreen?.OnNext();

        CurrentScreen = screen;
        CurrentScreen?.OnDisplayed();

        Title = CurrentScreen?.WindowTitle != null
          ? $"SMA Setup: {CurrentScreen.WindowTitle}"
          : "SMA Setup";
      }
      catch (Exception ex)
      {
        var errMsg = $"An exception occured while showing screen {screen?.ListTitle}";

        LogTo.Error(ex, errMsg);
        errMsg.ErrorMsgBox().Wait();
      }
    }

    private SMASetupScreenBase FindNextScreen()
    {
      return Screens.FirstOrDefault(s => s.IsSetup == false);
    }

    private void SMASetup_Loaded(object sender, System.EventArgs e)
    {
      ShowScreen(FindNextScreen());
    }

    private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter && NextCommand.CanExecute(null))
        NextCommand.Execute(null);

      else if (e.Key == Key.Escape)
        Close();
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
