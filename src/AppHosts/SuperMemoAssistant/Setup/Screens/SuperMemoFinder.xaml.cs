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
// Modified On:  2020/03/11 01:41
// Modified By:  Alexis

#endregion




using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using PropertyChanged;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Setup.Models;
using SuperMemoAssistant.SMA.Configs;
using SuperMemoAssistant.Sys.Collections.Microsoft.EntityFrameworkCore.ChangeTracking;
using SuperMemoAssistant.Sys.ComponentModel;
using SuperMemoAssistant.Sys.Windows.Input;
using SuperMemoFinderUtil = SuperMemoAssistant.SMA.Utils.SuperMemoFinder;

// ReSharper disable RedundantNameQualifier

namespace SuperMemoAssistant.Setup.Screens
{
  /// <summary>Make sure SuperMemo exe path is correct. Prompt user to input the path otherwise.</summary>
  public partial class SuperMemoFinder : SMASetupScreenBase, INotifyPropertyChangedEx
  {
    #region Properties & Fields - Non-Public

    private readonly CoreCfg       _startupCfg;
    private readonly NativeDataCfg _nativeDataCfg;
    private readonly bool          _initialIsSetup;

    #endregion




    #region Constructors

    public SuperMemoFinder(NativeDataCfg nativeDataCfg, CoreCfg startupCfg)
    {
      _nativeDataCfg = nativeDataCfg;
      _startupCfg    = startupCfg;

      _initialIsSetup = ShouldFindSuperMemo(startupCfg, nativeDataCfg) == false;
      Description = BuildDescriptionText();

      BrowseCommand = new RelayCommand(Browse);

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public SuperMemoFilePath         SMExeFilePath           { get; private set; }
    public ObservableHashSet<string> SMExeSuggestedFilePaths { get; private set; }

    public ICommand BrowseCommand { get; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    [DependsOn(nameof(SMExeFilePath))]
    public override bool IsSetup => (SMExeFilePath?.HasErrors ?? !_initialIsSetup) == false;

    /// <inheritdoc />
    public override string ListTitle => "SuperMemo";

    /// <inheritdoc />
    public override string WindowTitle => "SuperMemo .exe";
    /// <inheritdoc />
    public override string Description { get; }

    /// <inheritdoc />
    public bool IsChanged { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override void OnDisplayed()
    {
      if (SMExeSuggestedFilePaths == null)
      {
        SMExeFilePath = new SuperMemoFilePath(_startupCfg.SuperMemo.SMBinPath, _nativeDataCfg);
        SMExeSuggestedFilePaths = new ObservableHashSet<string>(SuperMemoFinderUtil.SearchSuperMemoInDefaultLocations()
                                                                                   .Select(fp => fp.FullPathWin));

        Task.Run(SearchForSuperMemo).RunAsync();
      }
    }

    /// <inheritdoc />
    public override void OnNext()
    {
      if (IsChanged)
      {
        _startupCfg.SuperMemo.SMBinPath = SMExeFilePath;
        SuperMemoAssistant.SMA.Core.Configuration.Save<CoreCfg>(_startupCfg).Wait();

        IsChanged = false;
      }
    }

    #endregion




    #region Methods

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

    private async Task SearchForSuperMemo()
    {
      var smFilePaths = await SuperMemoFinderUtil.SearchSuperMemoInWindowsIndex();

      Dispatcher.Invoke(() =>
      {
        SMExeSuggestedFilePaths.UnionWith(smFilePaths.Where(fp => fp.Exists())
                                                     .Select(fp => fp.FullPathWin));
      });
    }

    private bool ShouldFindSuperMemo(CoreCfg startupCfg, NativeDataCfg nativeDataCfg)
    {
      return string.IsNullOrWhiteSpace(startupCfg.SuperMemo.SMBinPath)
        || SuperMemoFinderUtil.CheckSuperMemoExecutable(
          nativeDataCfg,
          startupCfg.SuperMemo.SMBinPath,
          out _,
          out _) == false;
    }

    private string BuildDescriptionText()
    {
      var versions = _nativeDataCfg.Values
                                   .Select(nd => nd.SMVersion)
                                   .OrderBy(v => v)
                                   .Select(v => $"{v.Major:D2}.{v.Minor:D2}");
      var versionStr = string.Join("*, *", versions);

      return @$"Select your **SuperMemo executable** (.exe). You can:
- Use the <kbd>Browse</kbd> button,
- Double click on one of the suggested item from the list below.

If you need help, visit https://www.supermemo.wiki/sma   
**Supported versions**: *{versionStr}*";
    }

    #endregion
  }
}
