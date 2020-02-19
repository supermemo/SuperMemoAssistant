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
// Modified On:  2020/02/03 10:35
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.Sys;
using DistinctBy = MoreLinq.Extensions.DistinctByExtension;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace SuperMemoAssistant.Plugins
{
  public partial class PluginManager : PerpetualMarshalByRefObject, ISMAPluginManager, IDisposable
  {
    #region Constants & Statics

    public static PluginManager Instance { get; } = new PluginManager();

    #endregion




    #region Properties & Fields - Non-Public

    private readonly ObservableCollection<PluginInstance> _allPlugins = new ObservableCollection<PluginInstance>();
    private readonly ConcurrentDictionary<string, string> _interfaceChannelMap =
      new ConcurrentDictionary<string, string>();
    private readonly ConcurrentDictionary<Guid, PluginInstance> _runningPluginMap =
      new ConcurrentDictionary<Guid, PluginInstance>();

    #endregion




    #region Constructors

    private PluginManager()
    {
      AllPlugins = new ReadOnlyObservableCollection<PluginInstance>(_allPlugins);

      Core.SMA.OnSMStartedEvent += OnSMStarted;
      Core.SMA.OnSMStoppedEvent += OnSMStopped;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      if (IsDisposed)
        throw new InvalidOperationException("Already disposed");

      IsDisposed = true;

      Task.Run(StopPlugins).Wait();

      StopIpcServer();
    }

    #endregion




    #region Properties & Fields - Public

    public ReadOnlyObservableCollection<PluginInstance> AllPlugins { get; }

    public bool IsDisposed { get; private set; }

    #endregion




    #region Methods

    private async Task OnSMStarted(object sender, SMProcessArgs e)
    {
      LogTo.Debug($"Initializing {GetType().Name}");

      DirectoryEx.EnsureExists(SMAFileSystem.PluginPackageDir.FullPath);

      StartIpcServer();
      //StartMonitoringPlugins();

      await RefreshPlugins();
      await StartPlugins();
    }

    private void OnSMStopped(object        sender, SMProcessArgs e)
    {
      LogTo.Debug($"Cleaning up {GetType().Name}");

      StopPlugins().Wait();
      
      LogTo.Debug($"Cleaning up {GetType().Name}... Done");
    }

    public async Task StartPlugins()
    {
      var plugins = _allPlugins
                    .Where(pi => pi.Metadata.Enabled)
                    .OrderBy(pi => pi.Metadata.IsDevelopment)
                    .DistinctBy(pi => pi.Package.Id);
      var startTasks = plugins.Select(StartPlugin).ToList();

      await Task.WhenAll(startTasks);
    }

    public async Task StopPlugins()
    {
      var stopTasks = _runningPluginMap.Values.Select(StopPlugin);

      await Task.WhenAll(stopTasks);
    }

    private async Task RefreshPlugins()
    {
      LogTo.Information("Refreshing plugins.");

      await StopPlugins();

      _allPlugins.Clear();
      ScanPlugins(true)
        .Select(p => new PluginInstance(p))
        .Distinct()
        .ForEach(pi => _allPlugins.Add(pi));
    }

    private void OnPluginStarting(PluginInstance pluginInstance)
    {
      LogTo.Information($"Starting {pluginInstance.Denomination} {pluginInstance.Package.Id}.");

      _runningPluginMap[pluginInstance.OnStarting()] = pluginInstance;
    }

    private void OnPluginConnected(PluginInstance pluginInstance,
                                   ISMAPlugin     plugin)
    {
      LogTo.Information($"Connected {pluginInstance.Denomination} {pluginInstance.Package.Id}.");

      pluginInstance.OnConnected(plugin);
    }

    private void OnPluginStopping(PluginInstance pluginInstance)
    {
      LogTo.Information($"Stopping {pluginInstance.Denomination} {pluginInstance.Package.Id}.");

      pluginInstance.OnStopping();
    }

    private void OnPluginStopped(PluginInstance pluginInstance)
    {
      if (IsDisposed || pluginInstance.Status == PluginStatus.Stopped)
        return;

      bool crashed = false;

      try
      {
        if (pluginInstance.Process?.HasExited ?? false)
          crashed = pluginInstance.Process.ExitCode != 0;
      }
      catch
      {
        /* ignored */
      }

      LogTo.Information($"{pluginInstance.Denomination.CapitalizeFirst()} {pluginInstance.Metadata.PackageName} "
                        + $"has {(crashed ? "crashed" : "stopped")}");

      foreach (var interfaceType in pluginInstance.InterfaceChannelMap.Keys)
        UnregisterChannelType(interfaceType, pluginInstance.Guid, false);

      _runningPluginMap.TryRemove(pluginInstance.Guid, out _);

      pluginInstance.OnStopped();
    }

    #endregion
  }
}
