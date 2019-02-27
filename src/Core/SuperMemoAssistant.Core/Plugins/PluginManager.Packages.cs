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
// Created On:   2019/02/25 22:02
// Modified On:  2019/02/26 01:31
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
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

    public async Task<bool> InstallPlugin(
      PluginMetadata pluginMetadata,
      NuGetVersion   version    = null,
      bool           allowPrerelease = false)
    {
      var pm = PackageManager;

      if (pm.FindInstalledPluginById(pluginMetadata.PackageName) != null)
        throw new ArgumentException($"Package {pluginMetadata.PackageName} is already installed");

      var success = await pm.InstallPluginAsync(
        pluginMetadata.PackageName,
        pluginMetadata,
        new VersionRange(version),
        allowPrerelease);
      
      if (success == false)
        return false;

      var pluginPackage = pm.FindInstalledPluginById(pluginMetadata.PackageName);

      if (pluginPackage == null)
        throw new InvalidOperationException($"Package {pluginMetadata.PackageName} installed successfully but couldn't be found");
      
      _allPlugins.Add(new PluginInstance(pluginPackage));

      return true;
    }

    public async Task UninstallPlugin(PluginInstance pluginInstance)
    {
      if (pluginInstance.Metadata.IsDevelopment)
        throw new ArgumentException($"Cannot uninstall a development plugin");

      await StopPlugin(pluginInstance);

      using (await pluginInstance.Lock.LockAsync())
      {
        var pm      = PackageManager;
        var success = await pm.UninstallPluginAsync(pluginInstance.Package.Id);

        if (success)
          _allPlugins.Remove(pluginInstance);
      }
    }

    public IEnumerable<PluginPackage<PluginMetadata>> ScanPlugins(bool includeDev)
    {
      IEnumerable<PluginPackage<PluginMetadata>> plugins = ScanInstalledPlugins();

      if (includeDev == false)
        return plugins;

      var devPlugins = ScanDevelopmentPlugins();

      return devPlugins.Concat(plugins);
    }

    public IEnumerable<PluginPackage<PluginMetadata>> ScanInstalledPlugins()
    {
      var pm = PackageManager;

      lock (pm)
        return pm.GetInstalledPlugins();
    }

    public List<PluginPackage<PluginMetadata>> ScanDevelopmentPlugins()
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

    #endregion
  }
}
