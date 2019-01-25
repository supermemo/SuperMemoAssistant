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
// Modified On:  2019/01/25 23:01
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet.Project
{
  /// <summary>This primarily exists to intercept package installations and store their paths</summary>
  internal class NuGetPluginProject<TMeta> : FolderNuGetProject
  {
    #region Properties & Fields - Non-Public

    private readonly NuGetFramework                                       _currentFramework;
    private readonly Func<NuGetPluginProject<TMeta>, NuGetPackageManager> _packageManagerCreator;

    private bool _isInstalled;

    #endregion




    #region Constructors

    public NuGetPluginProject(Func<NuGetPluginProject<TMeta>, NuGetPackageManager> packageManagerCreator,
                              NuGetFramework                                       currentFramework,
                              DirectoryPath                                        packageDirPath,
                              PluginPackage<TMeta>                                 plugin,
                              bool                                                 isInstalled)
      : base(packageDirPath.FullPath)
    {
      Plugin                 = plugin;
      _packageManagerCreator = packageManagerCreator;
      _currentFramework      = currentFramework;
      _isInstalled           = isInstalled;
    }

    #endregion




    #region Properties & Fields - Public

    public PluginPackage<TMeta> Plugin { get; }

    #endregion




    #region Methods Impl

    // This gets called for every package install, including dependencies, and is our only chance to handle dependency PackageIdentity instances
    // If the package is already installed, returns false.
    public override Task<bool> InstallPackageAsync(
      PackageIdentity        packageIdentity,
      DownloadResourceResult downloadResourceResult,
      INuGetProjectContext   nuGetProjectContext,
      CancellationToken      token)
    {
      LogTo.Information(
        $"Installing plugin or dependency {packageIdentity.Id} {(packageIdentity.HasVersion ? packageIdentity.Version.ToNormalizedString() : string.Empty)}");

      if (packageIdentity.Id != Plugin.Identity.Id)
        Plugin.AddDependency(packageIdentity);

      return base.InstallPackageAsync(packageIdentity, downloadResourceResult, nuGetProjectContext, token);
    }

    public override Task<bool> UninstallPackageAsync(PackageIdentity      packageIdentity,
                                                     INuGetProjectContext nuGetProjectContext,
                                                     CancellationToken    token)
    {
      LogTo.Information(
        $"Uninstalling plugin or dependency {packageIdentity.Id} {(packageIdentity.HasVersion ? packageIdentity.Version.ToNormalizedString() : string.Empty)}");

      if (packageIdentity.Id != Plugin.Identity.Id)
        Plugin.RemoveDependency(packageIdentity);

      return base.UninstallPackageAsync(packageIdentity, nuGetProjectContext, token);
    }

    public override Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
    {
      var packages = _isInstalled
        ? Plugin.PluginAndDependencies
        : Plugin.Dependencies;

      return Task.FromResult(
        packages.Select(
          p => new PackageReference(
            p.Identity,
            _currentFramework,
            true,
            false,
            false))
      );
    }

    #endregion




    #region Methods

    public async Task UninstallPluginAsync(
      INuGetProjectContext projectContext,
      bool                 removeDependencies = true,
      bool                 forceRemove        = false,
      CancellationToken    cancellationToken  = default)
    {
      var uninstallContext = new UninstallationContext(removeDependencies, forceRemove);

      await CreatePackageManager().UninstallPackageAsync(
        this,
        Plugin.Id,
        uninstallContext,
        projectContext,
        cancellationToken);
    }

    public async Task InstallPluginAsync(
      INuGetProjectContext          projectContext,
      IEnumerable<SourceRepository> sourceRepositories,
      bool                          allowPrereleaseVersions = false,
      CancellationToken             cancellationToken       = default)
    {
      ResolutionContext resolutionContext = new ResolutionContext(
        DependencyBehavior.Lowest, allowPrereleaseVersions, false, VersionConstraints.None);

      await CreatePackageManager().InstallPackageAsync(
        this,
        Plugin.Identity,
        resolutionContext,
        projectContext,
        sourceRepositories,
        Array.Empty<SourceRepository>(),
        cancellationToken).ConfigureAwait(false);

      _isInstalled = true;
    }

    public NuGetPackageManager CreatePackageManager()
    {
      return _packageManagerCreator(this);
    }

    #endregion
  }
}
