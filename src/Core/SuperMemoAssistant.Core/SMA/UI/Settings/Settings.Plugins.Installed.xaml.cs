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
// Modified On:  2020/03/05 23:47
// Modified By:  Alexis

#endregion




using System.Windows;
using System.Windows.Controls;
using SuperMemoAssistant.SMA.UI.ViewModels;

namespace SuperMemoAssistant.SMA.UI.Settings
{
  /// <summary>
  ///   Show all plugins installed for the current user. User can choose to start, stop,
  ///   uninstall, update, or show the settings for each plugin
  /// </summary>
  public partial class InstalledPluginSettings : UserControl
  {
    #region Properties & Fields - Non-Public

    /// <summary>
    ///   This View Model is a singleton to preserve the state of the package manager across UI
    ///   navigation operations
    /// </summary>
    private PluginManagerViewModel ViewModel => PluginManagerViewModel.Instance;

    #endregion




    #region Constructors

    public InstalledPluginSettings()
    {
      DataContext = ViewModel;
      InitializeComponent();

      Unloaded += OnControlUnloaded;
    }

    #endregion




    #region Methods

    /// <summary>Cleanup this control when it is unloaded</summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnControlUnloaded(object sender, RoutedEventArgs e)
    {
      //ViewModel.CancelTasks(); // TODO: Only on Cancel button
    }

    /// <summary>
    ///   Automatically scroll to bottom when text is updated. TextBox is buggy as hell, and it
    ///   seems impossible to preserve the vertical scrollbar position on update. The TextBox will
    ///   scroll to the top when it is focused.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void tbOperationLogs_TextChanged(object sender, TextChangedEventArgs e)
    {
      tbOperationLogs.ScrollToEnd();
      tbOperationLogs.CaretIndex = tbOperationLogs.Text.Length;
    }

    #endregion
  }
}
