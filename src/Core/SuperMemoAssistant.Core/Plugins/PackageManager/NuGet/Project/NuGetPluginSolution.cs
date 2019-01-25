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
// Created On:   2019/01/25 15:56
// Modified On:  2019/01/25 22:42
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet.Project
{
  internal class NuGetPluginSolution<TMeta> : ISolutionManager
  {
    #region Properties & Fields - Non-Public

    private readonly NuGetFramework                                _currentFramework;
    private readonly DirectoryPath                                 _packageDirPath;
    private readonly NuGetInstalledPluginRepository<TMeta>         _pluginRepo;
    private readonly Dictionary<string, NuGetPluginProject<TMeta>> _projectMap;
    private readonly ISettings                                     _settings;
    private readonly SourceRepositoryProvider                      _sourceRepositories;

    #endregion




    #region Constructors

    public NuGetPluginSolution(DirectoryPath                         pluginDirPath,
                               DirectoryPath                         packageDirPath,
                               NuGetInstalledPluginRepository<TMeta> pluginRepo,
                               SourceRepositoryProvider              sourceRepositories,
                               ISettings                             settings,
                               NuGetFramework                        currentFramework)
    {
      SolutionDirectory   = pluginDirPath.Collapse().FullPath;
      NuGetProjectContext = new NuGetProjectContext(settings);

      _packageDirPath     = packageDirPath;
      _pluginRepo         = pluginRepo;
      _sourceRepositories = sourceRepositories;
      _settings           = settings;
      _currentFramework   = currentFramework;

      _projectMap = _pluginRepo.Plugins.Select(
        p => new NuGetPluginProject<TMeta>(
          CreatePackageManager,
          _currentFramework,
          _packageDirPath,
          p,
          true)
      ).ToDictionary(k => k.Plugin.Id);
    }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public string SolutionDirectory { get; }
    /// <inheritdoc />
    public bool IsSolutionOpen => true;
    /// <inheritdoc />
    public INuGetProjectContext NuGetProjectContext { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public Task<bool> IsSolutionAvailableAsync() => Task.FromResult(IsSolutionOpen);

    /// <inheritdoc />
    public Task<IEnumerable<NuGetProject>> GetNuGetProjectsAsync()
    {
      return Task.FromResult<IEnumerable<NuGetProject>>(_projectMap.Values);
    }

    /// <inheritdoc />
    public Task<string> GetNuGetProjectSafeNameAsync(NuGetProject nuGetProject)
    {
      if (!(nuGetProject is NuGetPluginProject<TMeta> pluginProject))
        throw new InvalidOperationException(
          $"Invalid project type {nuGetProject.GetType().FullName}. Should be {typeof(NuGetPluginProject<TMeta>).FullName}.");

      return Task.FromResult(pluginProject.Plugin.Id);
    }

    /// <inheritdoc />
    public Task<NuGetProject> GetNuGetProjectAsync(string projectName)
    {
      return Task.FromResult<NuGetProject>(_projectMap.SafeGet(projectName));
    }

    /// <inheritdoc />
    public void OnActionsExecuted(IEnumerable<ResolvedAction> actions)
    {
      ActionsExecuted?.Invoke(this, new ActionsExecutedEventArgs(actions));
    }

    /// <inheritdoc />
    public void EnsureSolutionIsLoaded() { }

    /// <inheritdoc />
    public Task<bool> DoesNuGetSupportsAnyProjectAsync()
    {
      return Task.FromResult(true);
    }

    #endregion




    #region Methods

    public async Task<bool> UninstallPluginAsync(
      string            pluginId,
      bool              removeDependencies = true,
      bool              forceRemove        = false,
      CancellationToken cancellationToken  = default)
    {
      var project = GetPluginProject(pluginId);

      if (project == null)
      {
        LogTo.Information($"No such plugin project {pluginId} exists for uninstallation");

        return false;
      }

      await project.UninstallPluginAsync(
        NuGetProjectContext,
        removeDependencies,
        forceRemove,
        cancellationToken);
      
      _pluginRepo.RemovePlugin(project.Plugin.Identity);
      _projectMap.Remove(pluginId);

      return await _pluginRepo.SaveAsync();
    }

    public async Task<bool> InstallPluginAsync(
      PackageIdentity   packageIdentity,
      TMeta             metadata                = default,
      bool              allowPrereleaseVersions = false,
      CancellationToken cancellationToken       = default)
    {
      if (_projectMap.ContainsKey(packageIdentity.Id))
        throw new ArgumentException($"Project {packageIdentity.Id} already exists");

      NuGetPluginProject<TMeta> project;

      using (var pluginInstallSession = _pluginRepo.AddPlugin(packageIdentity, metadata))
      {
        project = new NuGetPluginProject<TMeta>(
          CreatePackageManager,
          _currentFramework,
          _packageDirPath.FullPath,
          pluginInstallSession.Plugin,
          false);

        await project.InstallPluginAsync(
          NuGetProjectContext,
          _sourceRepositories.GetRepositories(),
          allowPrereleaseVersions,
          cancellationToken);

        pluginInstallSession.Success = true;
      }

      _projectMap[packageIdentity.Id] = project;

      return await _pluginRepo.SaveAsync();
    }


    public NuGetPluginProject<TMeta> GetPluginProject(PluginPackage<TMeta> plugin)
    {
      return GetPluginProject(plugin.Id);
    }

    public NuGetPluginProject<TMeta> GetPluginProject(string packageId)
    {
      return _projectMap.SafeGet(packageId);
    }

    private NuGetPackageManager CreatePackageManager(NuGetPluginProject<TMeta> project)
    {
      return new NuGetPackageManager(
        _sourceRepositories,
        _settings,
        this,
        new NuGetDeleteOnRestartManager())
      {
        PackagesFolderNuGetProject = project
      };
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event EventHandler<ActionsExecutedEventArgs> ActionsExecuted;
    /// <inheritdoc />
    public event EventHandler<NuGetEventArgs<string>> AfterNuGetCacheUpdated;
    /// <inheritdoc />
    public event EventHandler<NuGetProjectEventArgs> AfterNuGetProjectRenamed;
    /// <inheritdoc />
    public event EventHandler<NuGetProjectEventArgs> NuGetProjectAdded;
    /// <inheritdoc />
    public event EventHandler<NuGetProjectEventArgs> NuGetProjectRemoved;
    /// <inheritdoc />
    public event EventHandler<NuGetProjectEventArgs> NuGetProjectRenamed;
    /// <inheritdoc />
    public event EventHandler<NuGetProjectEventArgs> NuGetProjectUpdated;
    /// <inheritdoc />
    public event EventHandler SolutionClosed;
    /// <inheritdoc />
    public event EventHandler SolutionClosing;
    /// <inheritdoc />
    public event EventHandler SolutionOpened;
    /// <inheritdoc />
    public event EventHandler SolutionOpening;

    #endregion
  }
}
