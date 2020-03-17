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
// Modified On:  2020/03/17 13:17
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SuperMemoAssistant.SMA.UI.Controls
{
  /// <summary>Displays the change log and some additional info, pretty self-explanatory</summary>
  public partial class ChangeLogControl : UserControl, INotifyPropertyChanged
  {
    #region Constants & Statics

    public const string ChangeLogsAssemblyPath = "SuperMemoAssistant.Resources.ChangeLogs";
    
    private static readonly Regex RE_RemoveFileHeader = new Regex("^[^\\[]*", RegexOptions.Compiled);

    #endregion




    #region Constructors

    public ChangeLogControl()
    {
      ReleaseName = "SMA " + Core.SMAVersion;
      ChangeLog   = LoadChangeLogs() ?? "No change logs available at the moment.";

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public string ReleaseName { get; }

    public string ChangeLog { get; }

    #endregion




    #region Methods

    /// <summary>Read the ChangeLogs embedded resource</summary>
    /// <returns></returns>
    public static string LoadChangeLogs()
    {
      string changeLogs;

      using (Stream stream = typeof(ChangeLogControl).Assembly.GetManifestResourceStream(ChangeLogsAssemblyPath))
      using (StreamReader reader = new StreamReader(stream))
        changeLogs = reader.ReadToEnd();

      changeLogs = RE_RemoveFileHeader.Replace(changeLogs, string.Empty);

      if (string.IsNullOrWhiteSpace(changeLogs))
        return null;

      return "Change logs\n\n\n" + changeLogs;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
      e.Handled = true;
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
