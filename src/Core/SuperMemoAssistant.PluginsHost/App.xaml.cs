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
// Created On:   2018/12/27 01:27
// Modified On:  2018/12/30 05:17
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Windows;

namespace SuperMemoAssistant.PluginsHost
{
  /// <summary>Interaction logic for App.xaml</summary>
  public partial class App : Application
  {
    #region Constants & Statics

    public static readonly List<string> MahAppsResourceDictionaries = new List<string>
      { };
    /*
      "pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml",
      "pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml",
      "pack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml",
      "pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml",
      "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml",
      "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.Indigo.xaml",
      "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Pink.xaml",
      "pack://application:,,,/Forge.Forms;component/Themes/Material.xaml"
      "pack://application:,,,/Forge.Forms;component/Themes/Metro.xaml"
     */

    #endregion




    #region Constructors

    public App()
    {
      Startup += App_Startup;
    }

    #endregion




    #region Methods

    private void App_Startup(object           sender,
                             StartupEventArgs e)
    {
      foreach (var resDictSrc in MahAppsResourceDictionaries)
        Resources.MergedDictionaries.Add(new ResourceDictionary
        {
          Source = new Uri(resDictSrc,
                           UriKind.RelativeOrAbsolute)
        });
    }

    #endregion
  }
}
