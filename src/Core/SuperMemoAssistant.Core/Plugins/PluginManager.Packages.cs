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
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/22 13:39
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.IO;
using System.Linq;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Plugins.PackageManager;
using SuperMemoAssistant.Plugins.PackageManager.NuGet;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Plugins
{
  public partial class PluginManager
  {
    #region Properties & Fields - Non-Public

    private PluginPackageManager<PluginMetadata> _packageManager = null;

    private PluginPackageManager<PluginMetadata> PackageManager =>
      _packageManager ??
      (_packageManager = new PluginPackageManager<PluginMetadata>(
        SMAFileSystem.PluginDir,
        SMAFileSystem.PluginHomeDir,
        SMAFileSystem.PluginPackageDir,
        SMAFileSystem.PluginConfigFile,
        s => new NuGetSourceRepositoryProvider(s)));

    #endregion




    #region Methods

    public IEnumerable<PluginPackage<PluginMetadata>> GetPlugins(bool includeDev)
    {
      IEnumerable<PluginPackage<PluginMetadata>> plugins;
      var                                        pm = PackageManager;

      lock (pm)
        plugins = pm.GetInstalledPlugins();

      if (includeDev == false)
        return plugins;

      var devPlugins = GetDevelopmentPlugins();

      return devPlugins.Concat(plugins.Where(p => devPlugins.Any(dp => dp.Id == p.Id) == false));
    }

    public List<PluginPackage<PluginMetadata>> GetDevelopmentPlugins()
    {
      var devPlugins = new List<PluginPackage<PluginMetadata>>();
      var devDir     = SMAFileSystem.PluginDevelopmentDir;

      if (devDir.Exists() == false)
      {
        devDir.Create();
        return devPlugins;
      }

      foreach (DirectoryPath devPluginDir in Directory.EnumerateDirectories(devDir.FullPath))
      {
        var pluginPkg = DevPluginPackage.Create(devPluginDir.Segments.Last());

        if (pluginPkg == null)
          continue;

        devPlugins.Add(pluginPkg);
      }

      return devPlugins;
    }

    public PluginPackage<PluginMetadata> GetDevelopmentPlugin(string pkgId)
    {
      return DevPluginPackage.Create(pkgId);
    }

    #endregion
  }
}
