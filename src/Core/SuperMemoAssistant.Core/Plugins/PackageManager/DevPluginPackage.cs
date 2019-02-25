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
// Created On:   2019/02/21 02:00
// Modified On:  2019/02/21 13:39
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Diagnostics;
using Anotar.Serilog;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Plugins.PackageManager.NuGet;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Plugins.PackageManager
{
  public class DevPluginPackage : PluginPackage<PluginMetadata>
  {
    #region Constructors

    protected DevPluginPackage(PackageIdentity identity,
                               PluginMetadata  metadata) : base(identity, metadata) { }

    #endregion




    #region Methods Impl
    public override DirectoryPath GetHomeDir()
    {
      var devDir = SMAFileSystem.PluginDevelopmentDir;

      return devDir.Combine(Id);
    }

    /// <inheritdoc />
    public override Package AddDependency(PackageIdentity packageIdentity)
    {
      throw new InvalidOperationException("Dev Plugin does not manage dependencies");
    }

    /// <inheritdoc />
    public override Package RemoveDependency(PackageIdentity packageIdentity)
    {
      throw new InvalidOperationException("Dev Plugin does not manage dependencies");
    }

    /// <inheritdoc />
    public override IEnumerable<FilePath> GetPluginAndDependenciesAssembliesFilePaths(FolderNuGetProject project,
                                                                                      NuGetFramework     targetFramework)
    {
      throw new InvalidOperationException("Dev Plugin does not manage file paths");
    }

    /// <inheritdoc />
    public override IEnumerable<DirectoryPath> GetPluginAndDependenciesContentDirectoryPaths(FolderNuGetProject project,
                                                                                             NuGetFramework     targetFramework)
    {
      throw new InvalidOperationException("Dev Plugin does not manage file paths");
    }

    /// <inheritdoc />
    public override bool PackageAndDependenciesExist(NuGetPackageManager packageManager)
    {
      throw new InvalidOperationException("Dev Plugin does not manage files");
    }

    #endregion




    #region Methods

    public static DevPluginPackage Create(string packageName)
    {
      var      devDir         = SMAFileSystem.PluginDevelopmentDir;
      FilePath pluginFilePath = devDir.Combine(packageName).CombineFile(packageName + ".dll");

      if (pluginFilePath.Exists() == false)
      {
        LogTo.Warning($"Couldn't find development plugin dll {pluginFilePath.FullPath}. Skipping.");
        return null;
      }

      FileVersionInfo pluginVersionInfo = FileVersionInfo.GetVersionInfo(pluginFilePath.FullPath);

      if (pluginVersionInfo.ProductName != packageName)
      {
        LogTo.Warning($"Development plugin Folder name {packageName} differs from Assembly name {pluginVersionInfo.ProductName}. Skipping.");
        return null;
      }

      PluginMetadata pluginMetadata = new PluginMetadata
      {
        Enabled       = true,
        DisplayName   = pluginVersionInfo.ProductName,
        PackageName   = pluginFilePath.FileNameWithoutExtension,
        Description   = "Development plugin",
        IsDevelopment = true,
      };
      return new DevPluginPackage(
        new PackageIdentity(pluginMetadata.PackageName,
                            global::NuGet.Versioning.NuGetVersion.Parse(pluginVersionInfo.FileVersion)),
        pluginMetadata
      );
    }

    #endregion
  }
}
