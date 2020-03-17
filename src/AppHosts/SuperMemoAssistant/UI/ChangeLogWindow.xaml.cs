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
// Modified On:  2020/03/15 18:27
// Modified By:  Alexis

#endregion




using System;
using System.Windows;
using System.Windows.Input;
using Anotar.Serilog;
using MahApps.Metro.Controls;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.SMA.Configs;

namespace SuperMemoAssistant.UI
{
  /// <summary>Interaction logic for ChangeLogWindow.xaml</summary>
  public partial class ChangeLogWindow : MetroWindow
  {
    #region Constructors

    private ChangeLogWindow()
    {
      InitializeComponent();
    }

    #endregion




    #region Methods

    public static void ShowIfUpdated(CoreCfg cfg)
    {
      try
      {
        if (SMAFileSystem.SMAChangeLogFile.Exists() == false)
          return;

        var changeLogCrc32 = FileEx.GetCrc32(SMAFileSystem.SMAChangeLogFile.FullPath);

        if (changeLogCrc32 != cfg.Updates.ChangeLogLastCrc32)
        {
          cfg.Updates.ChangeLogLastCrc32 = changeLogCrc32;
          SMA.Core.Configuration.Save(cfg).RunAsync();

          new ChangeLogWindow().ShowDialog();
        }
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "An exception occured while checking whether to display the change log at startup");
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
        Close();
    }

    #endregion
  }
}
