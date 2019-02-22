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
// Created On:   2019/01/20 08:05
// Modified On:  2019/01/25 22:53
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Linq;
using Anotar.Serilog;
using Newtonsoft.Json;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Sys.IO;

// ReSharper disable PossibleMultipleEnumeration

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet
{
  [JsonObject(MemberSerialization.OptIn)]
  public class PluginPackage<TMeta> : Package
  {
    #region Constructors

    public PluginPackage() { }

    public PluginPackage(PackageIdentity identity,
                         TMeta           metadata)
      : base(identity)
    {
      Metadata = metadata;
    }

    #endregion




    #region Properties & Fields - Public

    [JsonProperty]
    public TMeta Metadata { get; set; }

    [JsonProperty]
    public HashSet<Package> Dependencies { get; set; } = new HashSet<Package>();

    public IEnumerable<Package> PluginAndDependencies => new List<Package> { this }.Concat(Dependencies);

    #endregion




    #region Methods

    public virtual DirectoryPath GetHomeDir()
    {
      return SMAFileSystem.PluginHomeDir.Combine(Id);
    }

    public virtual Package AddDependency(PackageIdentity packageIdentity)
    {
      var dependency = new Package(packageIdentity);

      Dependencies.Add(dependency);

      return dependency;
    }

    public virtual Package RemoveDependency(PackageIdentity packageIdentity)
    {
      Package dependency = null;

      Dependencies.RemoveWhere(p =>
      {
        if (Equals(p.Identity, packageIdentity))
        {
          dependency = p;
          return true;
        }

        return false;
      });

      return dependency;
    }

    public virtual IEnumerable<FilePath> GetPluginAndDependenciesAssembliesFilePaths(FolderNuGetProject project,
                                                                             NuGetFramework     targetFramework)
    {
      return PluginAndDependencies.SelectMany(p => p.GetReferencedAssembliesFilePaths(project, targetFramework));
    }

    public virtual IEnumerable<DirectoryPath> GetPluginAndDependenciesContentDirectoryPaths(FolderNuGetProject project,
                                                                                    NuGetFramework     targetFramework)
    {
      return PluginAndDependencies.SelectMany(p => p.GetContentDirectoryPath(project, targetFramework));
    }

    /// <summary>Verifies that the plugin package and all of its dependencies exist locally</summary>
    public virtual bool PackageAndDependenciesExist(NuGetPackageManager packageManager)
    {
      // Check this package
      if (!packageManager.PackageExistsInPackagesFolder(Identity))
      {
        LogTo.Warning(
          $"Cached plugin package {Id} {Version} does not exist in packages folder");

        return false;
      }

      // Check dependencies
      foreach (var package in Dependencies)
        if (!package.PackageExists(packageManager))
        {
          LogTo.Warning(
            $"Cached package dependency {package.Id} {package.Version} "
            + $"of plugin {Id} {Version} does not exist in packages folder");

          return false;
        }

      return true;
    }

    #endregion
  }
}
