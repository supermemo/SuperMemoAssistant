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
// Created On:   2019/02/26 19:42
// Modified On:  2019/02/26 22:00
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using PluginManager.PackageManager.Models;
using SuperMemoAssistant.Plugins.Models;
using SuperMemoAssistant.Plugins.Services;

namespace SuperMemoAssistant.SMA.UI.Settings
{

  /// <summary>Interaction logic for Settings.xaml</summary>
  public partial class BrowsePluginSettings : UserControl
  {
    #region Constructors
    
    public BrowsePluginSettings()
    {
      DataContext = this;

      InitializeComponent();
    }

    public async Task RefreshOnlinePlugins()
    {
      var onlinePlugins = await PluginRepositoryService.Instance.ListPlugins().ConfigureAwait(true);

      //Plugins = onlinePlugins;
    }

    public List<LocalPluginPackage<PluginMetadata>> Plugins {get; private set;}

    #endregion
  }
}
