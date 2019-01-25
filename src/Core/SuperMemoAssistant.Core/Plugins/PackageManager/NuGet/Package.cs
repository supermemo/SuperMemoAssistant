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
// Created On:   2019/01/25 12:43
// Modified On:  2019/01/25 14:04
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using Anotar.Serilog;
using Newtonsoft.Json;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Versioning;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet
{
  /// <summary>Original from: https://github.com/Wyamio/Wyam/ Copyright (c) 2014 Dave Glick</summary>
  [JsonObject(MemberSerialization.OptIn)]
  public class Package
    : IEquatable<Package>, IComparable<Package>
  {
    #region Properties & Fields - Non-Public

    private PackageIdentity _packageIdentity;

    #endregion




    #region Constructors

    public Package() { }

    public Package(PackageIdentity identity)
    {
      Id      = identity.Id;
      Version = identity.Version.ToNormalizedString();
    }

    #endregion




    #region Properties & Fields - Public

    [JsonProperty]
    public string Id { get; set; }

    [JsonProperty]
    public string Version { get; set; }

    public PackageIdentity Identity => _packageIdentity ?? (_packageIdentity = new PackageIdentity(Id, NuGetVersion.Parse(Version)));

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != GetType())
        return false;

      return Equals((Package)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return Identity != null ? Identity.GetHashCode() : 0;
    }


    /// <inheritdoc />
    public int CompareTo(Package other)
    {
      if (ReferenceEquals(this, other))
        return 0;
      if (ReferenceEquals(null, other))
        return 1;

      return Comparer<PackageIdentity>.Default.Compare(Identity, other.Identity);
    }

    public bool Equals(Package other)
    {
      return other != null && Equals(Identity, other.Identity);
    }

    #endregion




    #region Methods

    /// <summary>Returns the package directory path if it exists, null otherwise</summary>
    public DirectoryPath GetPackageDirectoryPath(FolderNuGetProject project)
    {
      return project.GetInstalledPath(Identity);
    }

    /// <summary>Returns the .nupkg file path if it exists, null otherwise</summary>
    public FilePath GetPackageFilePath(FolderNuGetProject project)
    {
      return project.GetInstalledPackageFilePath(Identity);
    }

    /// <summary>Verifies that the package exist locally</summary>
    public bool PackageExists(NuGetPackageManager packageManager)
    {
      return packageManager.PackageExistsInPackagesFolder(Identity);
    }

    /// <summary>Returns the referenced .dll/.exe file paths</summary>
    public IEnumerable<FilePath> GetReferencedAssembliesFilePaths(FolderNuGetProject project,
                                                                  NuGetFramework     targetFramework)
    {
      var pkgPath       = GetPackageDirectoryPath(project);
      var archiveReader = GetArchiveReader(project);

      List<FrameworkSpecificGroup> referenceItems = archiveReader.GetReferenceItems().ToList();
      FrameworkSpecificGroup       referenceGroup = SelectFrameworkMostCompatibleGroup(targetFramework, referenceItems);

      if (referenceGroup != null)
      {
        LogTo.Verbose(
          $"Found compatible reference group {referenceGroup.TargetFramework.DotNetFrameworkName} for package {Identity}");

        foreach (FilePath assemblyPath in referenceGroup.Items
                                                        .Select(x => new FilePath(x))
                                                        .Where(x => x.FileName.Extension == ".dll" || x.FileName.Extension == ".exe")
                                                        .Select(pkgPath.CombineFile))
        {
          LogTo.Verbose($"Found NuGet reference {assemblyPath} from package {Identity}");

          yield return assemblyPath;
        }
      }
      else if (referenceItems.Count == 0)
      {
        // Only show a verbose message if there were no reference items (I.e., it's probably a content-only package or a metapackage and not a mismatch)
        LogTo.Verbose($"Could not find any reference items in package {Identity}");
      }
      else
      {
        LogTo.Verbose(
          $"Could not find compatible reference group for package {Identity} (found {string.Join(",", referenceItems.Select(x => x.TargetFramework.DotNetFrameworkName))})");
      }
    }

    public IEnumerable<DirectoryPath> GetContentDirectoryPath(FolderNuGetProject project,
                                                              NuGetFramework     targetFramework)
    {
      var pkgPath       = GetPackageDirectoryPath(project);
      var archiveReader = GetArchiveReader(project);

      FrameworkSpecificGroup contentGroup = SelectFrameworkMostCompatibleGroup(targetFramework, archiveReader.GetContentItems().ToList());

      if (contentGroup != null)
        foreach (DirectoryPath contentPath in contentGroup.Items
                                                          .Select(x => new FilePath(x).Segments[0])
                                                          .Distinct()
                                                          .Select(x => pkgPath.Combine(x)))
        {
          LogTo.Verbose(
            $"Found content path {contentPath} from compatible content group {contentGroup.TargetFramework.DotNetFrameworkName} from package {Identity}");

          yield return contentPath;
        }
    }

    private PackageArchiveReader GetArchiveReader(FolderNuGetProject project)
    {
      return new PackageArchiveReader(GetPackageFilePath(project).FullPath, null, null);
    }

    /// The following method is originally from the internal MSBuildNuGetProjectSystemUtility class
    private static FrameworkSpecificGroup SelectFrameworkMostCompatibleGroup(NuGetFramework               projectTargetFramework,
                                                                             List<FrameworkSpecificGroup> itemGroups)
    {
      FrameworkReducer reducer = new FrameworkReducer();
      NuGetFramework mostCompatibleFramework
        = reducer.GetNearest(projectTargetFramework, itemGroups.Select(i => i.TargetFramework));
      if (mostCompatibleFramework != null)
      {
        FrameworkSpecificGroup mostCompatibleGroup
          = itemGroups.FirstOrDefault(i => i.TargetFramework.Equals(mostCompatibleFramework));

        if (IsValidFrameworkGroup(mostCompatibleGroup))
          return mostCompatibleGroup;
      }

      return null;
    }

    /// The following method is originally from the internal MSBuildNuGetProjectSystemUtility class
    private static bool IsValidFrameworkGroup(FrameworkSpecificGroup frameworkSpecificGroup)
    {
      if (frameworkSpecificGroup != null)
        return frameworkSpecificGroup.HasEmptyFolder
          || frameworkSpecificGroup.Items.Any()
          || !frameworkSpecificGroup.TargetFramework.Equals(NuGetFramework.AnyFramework);

      return false;
    }

    public static bool operator ==(Package left,
                                   Package right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Package left,
                                   Package right)
    {
      return !Equals(left, right);
    }

    #endregion
  }
}
