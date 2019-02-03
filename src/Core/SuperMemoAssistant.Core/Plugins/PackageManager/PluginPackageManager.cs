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
// Created On:   2019/01/23 15:22
// Modified On:  2019/01/25 22:40
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using JetBrains.Annotations;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using SuperMemoAssistant.Plugins.PackageManager.NuGet;
using SuperMemoAssistant.Plugins.PackageManager.NuGet.Project;
using SuperMemoAssistant.Sys.IO;
using SourceRepositoryProvider = SuperMemoAssistant.Plugins.PackageManager.NuGet.SourceRepositoryProvider;

namespace SuperMemoAssistant.Plugins.PackageManager
{
  /// <summary>A wrapper around <see cref="NuGetPackageManager" /> to simplify package management.</summary>
  public class PluginPackageManager<TMeta>
  {
    #region Properties & Fields - Non-Public

    private readonly NuGetFramework                        _currentFramework;
    private readonly NuGetLogger                           _logger = new NuGetLogger();
    private readonly NuGetInstalledPluginRepository<TMeta> _pluginRepo;
    private readonly NuGetPluginSolution<TMeta>            _solution;
    private readonly SourceRepositoryProvider              _sourceRepositories;

    #endregion




    #region Constructors

    internal PluginPackageManager([NotNull] DirectoryPath                   pluginDirPath,
                                  [NotNull] DirectoryPath                   pluginHomeDirPath,
                                  [NotNull] DirectoryPath                   packageDirPath,
                                  [NotNull] FilePath                        configFilePath,
                                  Func<ISettings, SourceRepositoryProvider> providerCreator = null)
    {
      pluginDirPath  = pluginDirPath.Collapse();
      packageDirPath = packageDirPath.Collapse();

      if (pluginDirPath.Exists() == false)
        throw new ArgumentException($"Root path {pluginDirPath.FullPath} doesn't exist.");

      if (packageDirPath.Exists() == false)
        throw new ArgumentException($"Package path {packageDirPath.FullPath} doesn't exist.");

      if (configFilePath.Root.Exists() == false)
        throw new ArgumentException($"Config's root directory {configFilePath.Root.FullPath} doesn't exist.");

      var packageCacheTask = NuGetInstalledPluginRepository<TMeta>.LoadAsync(configFilePath);
      var settings         = Settings.LoadDefaultSettings(packageDirPath.FullPath, null, new MachineWideSettings());

      _currentFramework   = GetCurrentFramework();
      _sourceRepositories = providerCreator?.Invoke(settings) ?? new SourceRepositoryProvider(settings);
      _pluginRepo         = packageCacheTask.Result;
      _solution = new NuGetPluginSolution<TMeta>(
        pluginDirPath, pluginHomeDirPath, packageDirPath,
        _pluginRepo,
        _sourceRepositories,
        settings,
        _currentFramework
      );
    }

    #endregion




    #region Methods

    public NuGetFramework GetCurrentFramework()
    {
      Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
      string frameworkName = assembly.GetCustomAttributes(true)
                                     .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                                     .Select(x => x.FrameworkName)
                                     .FirstOrDefault();
      return frameworkName == null
        ? NuGetFramework.AnyFramework
        : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
    }

    /// <summary>
    ///   Adds the specified package source. Sources added this way will be searched before any
    ///   global sources.
    /// </summary>
    /// <param name="repository">The package source to add.</param>
    public SourceRepository AddRepository(string repository) => _sourceRepositories.CreateRepository(repository);

    /// <summary>Gets all installed versions of package <paramref name="packageId" />, if any.</summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="verify">
    ///   Whether to verify if each version, and its dependencies, are correctly
    ///   installed.
    /// </param>
    public IEnumerable<NuGetVersion> FindInstalledPluginVersions(string packageId,
                                                          bool   verify = false)
    {
      var pkgs = _pluginRepo.FindPackageById(packageId);

      if (verify)
        pkgs = pkgs.Where(p => _pluginRepo.IsPluginInstalled(p.Identity, _solution));

      return pkgs.Select(p => p.Identity.Version);
    }
    
    public PluginPackage<TMeta> FindInstalledPluginById(string pluginId) => _pluginRepo.FindPluginById(pluginId);

    public PluginPackage<TMeta> GetInstalledPlugin(PackageIdentity identity) => _pluginRepo[identity];

    public IEnumerable<PluginPackage<TMeta>> GetInstalledPlugins() => _pluginRepo.Plugins;

    /// <summary>
    ///   Gets assemblies (.dll, .exe) file paths referenced in given
    ///   <paramref name="packageIdentity" /> and its dependencies , for framework
    ///   <paramref name="targetFramework" />
    /// </summary>
    /// <param name="packageIdentity">Package to look for</param>
    /// <param name="dependenciesAssemblies"></param>
    /// <param name="targetFramework">
    ///   Target framework to match, or current assembly's framework if
    ///   <see langword="null" />
    /// </param>
    /// <param name="pluginAssemblies"></param>
    public void GetInstalledPluginAssembliesFilePath(PackageIdentity packageIdentity,
                                                                      out IEnumerable<FilePath> pluginAssemblies,
                                                                      out IEnumerable<FilePath> dependenciesAssemblies,
                                                                      NuGetFramework  targetFramework     = null)
    {
      var project = _solution.GetPluginProject(packageIdentity.Id);
      var plugin = _pluginRepo[packageIdentity];

      if (project == null || plugin == null)
        throw new ArgumentException($"No such plugin {packageIdentity.Id} {packageIdentity.Version.ToNormalizedString()}");

      pluginAssemblies = plugin.GetReferencedAssembliesFilePaths(project, targetFramework ?? _currentFramework);
      dependenciesAssemblies = plugin.Dependencies.SelectMany(i => i.GetReferencedAssembliesFilePaths(project, targetFramework ?? _currentFramework));
    }

    public Task<bool> UninstallPluginAsync(string            pluginId,
                                           bool              removeDependencies = true,
                                           bool              forceRemove        = false,
                                           CancellationToken cancellationToken  = default)
    {
      try
      {
        LogTo.Verbose($"Uninstall requested for plugin {pluginId}");

        return _solution.UninstallPluginAsync(
          pluginId,
          removeDependencies,
          forceRemove,
          cancellationToken);
      }
      catch (Exception ex)
      {
        LogTo.Error(
          $"Unexpected exception while uninstalling packages: {(ex is AggregateException aggEx ? string.Join("; ", aggEx.InnerExceptions.Select(x => x.Message)) : ex.Message)}");

        return Task.FromResult(false);
      }
    }

    public async Task<bool> UpdatePluginAsync()
    {
      await Task.Run(() => throw new NotImplementedException());

      return true;
    }

    public async Task<bool> InstallPluginAsync(
      string            packageId,
      TMeta             metadata                = default,
      VersionRange      versionRange            = null,
      bool              allowPrereleaseVersions = false,
      NuGetFramework    framework               = null,
      CancellationToken cancellationToken       = default)
    {
      try
      {
        var version = (await LookupMatchingVersion(packageId, versionRange, framework, cancellationToken).ConfigureAwait(false)).Max();

        if (version == null)
          return false;

        LogTo.Verbose($"Install requested for plugin {packageId} {version.ToNormalizedString()}");

        // Check if this package was already installed in a previous run
        PackageIdentity packageIdentity = new PackageIdentity(packageId, version);

        // If plugin exact version already exists, abort
        if (_pluginRepo.IsPluginInstalled(packageIdentity, _solution))
        {
          LogTo.Information("Already got plugin {packageId} {version.ToNormalizedString()}");

          return true;
        }

        // If plugin already exists in a different version, try to update it
        if (_pluginRepo.FindPluginById(packageId) != null)
        {
          LogTo.Information("Plugin already exist with a different version. Redirecting to UpdatePluginAsync.");

          return await UpdatePluginAsync();
        }

        // If plugin doesn't exist, go ahead and install it
        return await _solution.InstallPluginAsync(
          packageIdentity,
          metadata,
          allowPrereleaseVersions,
          cancellationToken);
      }
      catch (Exception ex)
      {
        LogTo.Error(
          $"Unexpected exception while installing packages: {(ex is AggregateException aggEx ? string.Join("; ", aggEx.InnerExceptions.Select(x => x.Message)) : ex.Message)}");

        throw;
      }
    }

    public Task<IEnumerable<NuGetVersion>> LookupMatchingVersion(
      string            packageId,
      VersionRange      versionRange      = null,
      NuGetFramework    framework         = null,
      CancellationToken cancellationToken = default)
    {
      return LookupMatchingVersion(_sourceRepositories, packageId, versionRange, framework, cancellationToken);
    }

    public Task<IEnumerable<NuGetVersion>> LookupMatchingVersion(
      ISourceRepositoryProvider provider,
      string                    packageId,
      VersionRange              versionRange      = null,
      NuGetFramework            framework         = null,
      CancellationToken         cancellationToken = default)
    {
      return LookupMatchingVersion(provider.GetRepositories(), packageId, versionRange, framework, cancellationToken);
    }

    public async Task<IEnumerable<NuGetVersion>> LookupMatchingVersion(
      IEnumerable<SourceRepository> repositories,
      string                        packageId,
      VersionRange                  versionRange      = null,
      NuGetFramework                framework         = null,
      CancellationToken             cancellationToken = default)
    {
      var tasks = repositories.Select(r => LookupMatchingVersion(r, packageId, versionRange, framework, cancellationToken));

      return await Task.WhenAll(tasks);
    }

    public async Task<NuGetVersion> LookupMatchingVersion(
      SourceRepository  sourceRepository,
      string            packageId,
      VersionRange      versionRange      = null,
      NuGetFramework    framework         = null,
      CancellationToken cancellationToken = default)
    {
      try
      {
        var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(cancellationToken);

        using (var sourceCacheContext = new SourceCacheContext())
        {
          var dependencyInfo =
            await dependencyInfoResource
              .ResolvePackages(packageId, framework ?? _currentFramework, sourceCacheContext, _logger, cancellationToken);

          return dependencyInfo
                 .Select(x => x.Version)
                 .Where(x => x != null && (versionRange == null || versionRange.Satisfies(x)))
                 .DefaultIfEmpty()
                 .Max();
        }
      }
      catch (Exception ex)
      {
        LogTo.Warning($"Could not get latest version for package {packageId} from source {sourceRepository}: {ex.Message}");

        return null;
      }
    }

    #endregion
  }
}
