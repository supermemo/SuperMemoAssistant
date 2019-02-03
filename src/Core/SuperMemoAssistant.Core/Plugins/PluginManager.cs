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
// Created On:   2018/05/30 12:47
// Modified On:  2019/01/26 05:55
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using Nito.AsyncEx;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Plugins.PackageManager.NuGet;
using SuperMemoAssistant.SuperMemo;
using SuperMemoAssistant.Sys;
using SysProcess = System.Diagnostics.Process;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace SuperMemoAssistant.Plugins
{
  public partial class PluginManager : SMMarshalByRefObject, ISMAPluginManager, IDisposable
  {
    #region Constants & Statics

    private const int PluginConnectTimeout = 8000;

    public static PluginManager Instance { get; } = new PluginManager();

    #endregion




    #region Properties & Fields - Non-Public

    private ConcurrentDictionary<ISMAPlugin, PluginInstance> InstanceMap { get; } =
      new ConcurrentDictionary<ISMAPlugin, PluginInstance>();

    #endregion




    #region Constructors

    private PluginManager()
    {
      SMA.Instance.OnSMStartedEvent += OnSMStarted;
      SMA.Instance.OnSMStoppedEvent += OnSMStopped;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      IsDisposed = true;

      Task.Run(UnloadPlugins).Wait();

      StopIpcServer();
    }

    #endregion




    #region Properties & Fields - Public

    public bool IsDisposed { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public bool GetAssembliesPathsForPlugin(string                  pluginId,
                                            out IEnumerable<string> pluginAssemblies,
                                            out IEnumerable<string> dependenciesAssemblies)
    {
      pluginAssemblies       = null;
      dependenciesAssemblies = null;

      try
      {
        LogTo.Information($"Fetching assemblies requested by plugin {pluginId}");

        if (StartInfoMap.ContainsKey(pluginId) == false)
        {
          LogTo.Warning($"Plugin {pluginId} unexpected for assembly request. Aborting");
          return false;
        }

        var pm = PackageManager;

        lock (pm)
        {
          var plugin = pm.FindInstalledPluginById(pluginId);

          if (plugin == null)
            throw new InvalidOperationException($"Cannot find requested plugin package {pluginId}");

          pm.GetInstalledPluginAssembliesFilePath(
            plugin.Identity,
            out var tmpPluginAssemblies,
            out var tmpDependenciesAssemblies);

          pluginAssemblies       = tmpPluginAssemblies.Select(p => p.FullPath);
          dependenciesAssemblies = tmpDependenciesAssemblies.Select(p => p.FullPath);
        }

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"An exception occured while returning assemblies path gor plugin {pluginId}");

        return false;
      }
    }

    /// <inheritdoc />
    public ISuperMemoAssistant ConnectPlugin(ISMAPlugin plugin,
                                             int        processId)
    {
      string pluginId = "N/A";

      try
      {
        pluginId = plugin.AssemblyName;

        LogTo.Information($"Connecting plugin {pluginId}");

        PluginStartInfo pluginStartInfo = StartInfoMap.SafeGet(pluginId);

        if (pluginStartInfo == null)
        {
          LogTo.Warning($"Plugin {pluginId} unexpected for connection. Aborting");
          return null;
        }

        pluginStartInfo.ConnectedEvent.Set();
        var pluginPackage = pluginStartInfo.Plugin;

        var pluginInst = InstanceMap[plugin] = new PluginInstance(plugin, pluginPackage.Metadata, processId);

        pluginInst.Process.Exited += (o,
                                      ev) => OnPluginStopped(pluginInst);

        return SMA.Instance;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"An exception occured while connecting plugin {pluginId}");

        return null;
      }
    }

    /// <inheritdoc />
    public string GetChannelNameForType(Type remoteInterface)
    {
      return null;
    }

    /// <inheritdoc />
    public IDisposable RegisterChannelForType(Type       remoteInterface,
                                              string     channelName,
                                              ISMAPlugin plugin)
    {
      return null;
    }

    #endregion




    #region Methods

    private void OnSMStarted(object        sender,
                             SMProcessArgs e)
    {
      DirectoryEx.EnsureExists(SMAFileSystem.PluginPackageDir.FullPath);

      StartIpcServer();
      StartMonitoringPlugins();

      Task.Run(LoadPlugins).Wait();
    }

    private void OnSMStopped(object        sender,
                             SMProcessArgs e)
    {
      Task.Run(UnloadPlugins).Wait();
    }

    public async Task ReloadPlugins()
    {
      await UnloadPlugins();
      await LoadPlugins();
    }

    /// <summary>
    /// Attempts to load <paramref name="pluginId"/>
    /// </summary>
    /// <param name="pluginId">Plugin's assembly name</param>
    /// <exception cref="InvalidOperationException">if plugin cannot be found</exception>
    public async Task LoadPlugin(string pluginId)
    {
      LogTo.Information($"Loading plugin {pluginId}");
      
      PluginPackage<PluginMetadata> plugin;
      var                                        pm = PackageManager;

      lock (pm)
        plugin = pm.FindInstalledPluginById(pluginId);

      if (plugin == null)
        throw new InvalidOperationException($"Could not find plugin package {pluginId}");

      await StartPlugin(plugin);
    }

    /// <summary>
    /// Attempts tu unload <paramref name="pluginId"/>
    /// </summary>
    /// <param name="pluginId">Plugin's assembly name</param>
    /// <returns><see langword="true"/> if successful, <see langword="false"/> otherwise</returns>
    public async Task<bool> UnloadPlugin(string pluginId)
    {
      LogTo.Information($"Unoading plugin {pluginId}");

      var pluginInstance = InstanceMap.Values.FirstOrDefault(pi => pi.Metadata.PackageName == pluginId);

      if (pluginInstance == null)
        return false;

      await StopPlugin(pluginInstance);

      return true;
    }

    private async Task LoadPlugins()
    {
      LogTo.Information("Loading plugins.");

      IEnumerable<PluginPackage<PluginMetadata>> plugins;
      var                                        pm = PackageManager;

      lock (pm)
        plugins = pm.GetInstalledPlugins();

      var startTasks = plugins.Select(StartPlugin);

      await Task.WhenAll(startTasks);
    }

    private async Task UnloadPlugins()
    {
      var stopTasks = InstanceMap.Values.Select(StopPlugin);

      await Task.WhenAll(stopTasks);
    }

    private void OnPluginStopped(PluginInstance pluginInstance)
    {
      lock (pluginInstance)
      {
        if (IsDisposed || pluginInstance.ExitHandled)
          return;

        // TODO: Notify user if plugin crashed

        pluginInstance.ExitHandled = true;

        InstanceMap.TryRemove(pluginInstance.Plugin, out _);
      }
    }

    #endregion




    private class PluginStartInfo
    {
      #region Constructors

      public PluginStartInfo(PluginPackage<PluginMetadata> plugin)
      {
        Plugin = plugin;
      }

      #endregion




      #region Properties & Fields - Public

      public PluginPackage<PluginMetadata> Plugin         { get; }
      public AsyncManualResetEvent         ConnectedEvent { get; } = new AsyncManualResetEvent(false);

      #endregion
    }
  }
}
