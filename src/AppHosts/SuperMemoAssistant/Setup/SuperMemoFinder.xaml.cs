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
// Modified On:  2020/02/03 16:21
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using PropertyChanged;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.Sys.Windows.Input;
using SuperMemoFinderUtil = SuperMemoAssistant.SMA.Utils.SuperMemoFinder;

// ReSharper disable RedundantNameQualifier

namespace SuperMemoAssistant.Setup
{
  /// <summary>Interaction logic for SuperMemoFinder.xaml</summary>
  public partial class SuperMemoFinder : MetroWindow, INotifyPropertyChanged
  {
    #region Properties & Fields - Non-Public

    private readonly CoreCfg    _startupCfg;
    private readonly NativeDataCfg _nativeDataCfg;

    #endregion




    #region Constructors

    public SuperMemoFinder(NativeDataCfg nativeDataCfg, CoreCfg startupCfg)
    {
      _nativeDataCfg = nativeDataCfg;
      _startupCfg    = startupCfg;

      SMExeFilePath = new SuperMemoFilePath(startupCfg.SuperMemo.SMBinPath, nativeDataCfg);
      SMExeSuggestedFilePaths = SuperMemoFinderUtil.SearchSuperMemoInDefaultLocations()
                                                   .Select(fp => fp.FullPathWin)
                                                   .ToHashSet();

      AcceptCommand = new RelayCommand(Accept, CanAcceptExecute);
      BrowseCommand = new RelayCommand(Browse);

      BuildDescriptionText();

      Task.Run(SearchForSuperMemo).RunAsync();

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public string            DescriptionText         { get; set; }
    public SuperMemoFilePath SMExeFilePath           { get; private set; }
    public HashSet<string>   SMExeSuggestedFilePaths { get; }

    [DependsOn(nameof(SMExeFilePath))]
    public ICommand AcceptCommand { get; }
    public ICommand BrowseCommand { get; }

    #endregion




    #region Methods

    private bool CanAcceptExecute()
    {
      return (SMExeFilePath?.HasErrors ?? true) == false;
    }

    private void Accept()
    {
      _startupCfg.SuperMemo.SMBinPath = SMExeFilePath;
      SuperMemoAssistant.SMA.Core.Configuration.Save<CoreCfg>(_startupCfg).Wait();

      DialogResult = true;

      Close();
    }

    private void Browse()
    {
      OpenFileDialog dlg = new OpenFileDialog
      {
        DefaultExt = ".exe",
        Filter     = "Executable (*.exe)|*.exe|All files (*.*)|*.*"
      };

      var filePath = dlg.ShowDialog().GetValueOrDefault(false)
        ? dlg.FileName
        : null;

      if (filePath != null)
        SMExeFilePath = new SuperMemoFilePath(filePath, _nativeDataCfg);
    }

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (lbPaths.SelectedItem is string filePath)
        SMExeFilePath = new SuperMemoFilePath(filePath, _nativeDataCfg);
    }

    private void BuildDescriptionText()
    {
      var versions = _nativeDataCfg.Values
                                   .Select(nd => nd.SMVersion)
                                   .OrderBy(v => v);
      var versionStr = string.Join("*, *", versions);

      DescriptionText = @$"Select your **SuperMemo executable** (.exe). You can:
- Use the <kbd>Browse</kbd> button,
- Double click on one of the suggested item from the list below.

If you need help, visit https://www.supermemo.wiki/sma   
> **Supported versions**: *{versionStr}*";
    }

    private async Task SearchForSuperMemo()
    {
      var smFilePaths = await SuperMemoFinderUtil.SearchSuperMemoInWindowsIndex();

      SMExeSuggestedFilePaths.UnionWith(smFilePaths.Where(fp => fp.Exists())
                                                   .Select(fp => fp.FullPathWin));
    }

    private void MetroWindow_Loaded(object sender, EventArgs e)
    {
      //DialogResult = false;
    }

    private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter && AcceptCommand.CanExecute(null))
        AcceptCommand.Execute(null);

      else if (e.Key == Key.Escape)
        Close();
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
