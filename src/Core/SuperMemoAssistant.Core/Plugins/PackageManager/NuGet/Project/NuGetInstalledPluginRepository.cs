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
// Created On:   2019/01/25 12:37
// Modified On:  2019/01/25 22:56
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NuGet.Packaging.Core;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet.Project
{
  /// <summary>Original from: https://github.com/Wyamio/Wyam/ Copyright (c) 2014 Dave Glick</summary>
  [JsonObject(MemberSerialization.OptIn)]
  internal class NuGetInstalledPluginRepository<TMeta> : IDisposable
  {
    #region Properties & Fields - Non-Public

    private NuGetPluginInstallSession _currentlyInstallingPlugin = null;

    protected Dictionary<PackageIdentity, PluginPackage<TMeta>>
      _identityPluginMap = new Dictionary<PackageIdentity, PluginPackage<TMeta>>();

    [JsonProperty(PropertyName = "Plugins")]
    private IEnumerable<PluginPackage<TMeta>> ConfigPlugins
    {
      get => _identityPluginMap.Values;
      set => _identityPluginMap = value.ToDictionary(p => p.Identity);
    }

    #endregion




    #region Constructors

    public NuGetInstalledPluginRepository() { }

    public void Dispose()
    {
      SaveAsync().Wait();
    }

    #endregion




    #region Properties & Fields - Public

    public FilePath FilePath { get; private set; }

    public PluginPackage<TMeta> this[PackageIdentity pi] => _identityPluginMap.SafeGet(pi);

    public IEnumerable<PluginPackage<TMeta>> Plugins => _identityPluginMap.Values;

    public IEnumerable<Package> AllPackages =>
      _identityPluginMap.Values.Concat(_identityPluginMap.Values.SelectMany(p => p.Dependencies)).Distinct();

    public bool IsInstalling => _currentlyInstallingPlugin != null && _currentlyInstallingPlugin.CurrentlyInstalling;

    #endregion




    #region Methods

    public static async Task<NuGetInstalledPluginRepository<TMeta>> LoadAsync(
      [NotNull] FilePath filePath)
    {
      NuGetInstalledPluginRepository<TMeta> repo = null;

      if (File.Exists(filePath.FullPath))
        try
        {
          repo          = await JsonEx.DeserializeFromFileAsync<NuGetInstalledPluginRepository<TMeta>>(filePath);
          repo.FilePath = filePath;
        }
        catch (Exception ex)
        {
          LogTo.Error($"Error while reading packages file: {ex.Message}");

          // TODO: Rebuild cache
        }

      return repo ?? new NuGetInstalledPluginRepository<TMeta> { FilePath = filePath };
    }

    public async Task<bool> SaveAsync()
    {
      try
      {
        await this.SerializeToFileAsync(FilePath, Formatting.Indented);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Failed to save Installed Packages Cache");

        return false;
      }

      return true;
    }

    public PluginPackage<TMeta> FindPluginById(string packageId)
    {
      return _identityPluginMap.FirstOrDefault(kv => kv.Key.Id == packageId).Value;
    }

    public IEnumerable<Package> FindPackageById(string packageId)
    {
      return AllPackages.Where(p => p.Id == packageId);
    }

    public List<Package> GetPackageAndDependencies(PackageIdentity pi)
    {
      var package = this[pi];

      if (package == null)
        return null;

      var ret = new List<Package>()
      {
        package
      };

      ret.AddRange(package.Dependencies);

      return ret;
    }

    public bool ContainsPackage(PackageIdentity packageIdentity)
    {
      return AllPackages.FirstOrDefault(p => Equals(p.Identity, packageIdentity)) != null;
    }

    public bool ContainsPlugin(PackageIdentity pluginIdentity)
    {
      return _identityPluginMap.ContainsKey(pluginIdentity);
    }

    /// <summary>
    ///   Verifies that a package has been previously installed as well as currently existing
    ///   locally with all dependencies.
    /// </summary>
    public bool IsPluginInstalled(PackageIdentity            packageIdentity,
                                  NuGetPluginSolution<TMeta> solution)
    {
      var plugin = _identityPluginMap.SafeGet(packageIdentity);

      if (plugin == null)
        return false;

      var project = solution.GetPluginProject(plugin);

      if (project == null)
        throw new InvalidOperationException($"Could not acquire project for existing plugin {plugin.Id}.");

      return plugin.PackageAndDependenciesExist(project.CreatePackageManager());
    }

    public NuGetPluginInstallSession AddPlugin(PackageIdentity identity,
                                               TMeta           metadata = default)
    {
      if (IsInstalling)
      {
        _currentlyInstallingPlugin.Plugin.AddDependency(identity);

        return null;
      }

      // This is a new top-level installation, so add to the root
      var pkg = new PluginPackage<TMeta>(identity, metadata);

      _currentlyInstallingPlugin = new NuGetPluginInstallSession(this, pkg);

      return _currentlyInstallingPlugin;
    }

    public bool RemovePlugin(PackageIdentity packageIdentity)
    {
      return _identityPluginMap.Remove(packageIdentity);
    }

    #endregion




    public class NuGetPluginInstallSession : IDisposable
    {
      #region Properties & Fields - Non-Public

      private readonly NuGetInstalledPluginRepository<TMeta> _repo;

      #endregion




      #region Constructors

      public NuGetPluginInstallSession(NuGetInstalledPluginRepository<TMeta> repo,
                                       PluginPackage<TMeta>                  plugin)
      {
        _repo               = repo;
        Plugin              = plugin;
        CurrentlyInstalling = true;
      }

      public void Dispose()
      {
        CurrentlyInstalling = false;

        if (Success)
          _repo._identityPluginMap.Add(Plugin.Identity, Plugin);
      }

      #endregion




      #region Properties & Fields - Public

      public PluginPackage<TMeta> Plugin { get; }

      public bool CurrentlyInstalling { get; private set; }

      public bool Success { get; set; } = false;

      #endregion
    }
  }
}
