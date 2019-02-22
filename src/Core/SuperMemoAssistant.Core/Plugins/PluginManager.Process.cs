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
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/21 13:57
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Plugins.PackageManager.NuGet;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.Plugins
{
  public partial class PluginManager
  {
    #region Constants & Statics
    
    private const int PluginStopTimeout = 3000;
    
#if DEBUG
    private const int PluginConnectTimeout = 60000;
#else
    private const int PluginConnectTimeout = 5000;
#endif

    #endregion




    #region Properties & Fields - Non-Public

    private ConcurrentDictionary<string, PluginStartInfo> StartInfoMap { get; } =
      new ConcurrentDictionary<string, PluginStartInfo>();

    #endregion




    #region Methods

    private void StartMonitoringPlugins()
    {
      Task.Factory.StartNew(MonitorPlugins,
                            TaskCreationOptions.LongRunning);
    }

    private void MonitorPlugins()
    {
      int sleepCounter = 0;

      while (IsDisposed == false)
      {
        if (sleepCounter > 6)
        {
          try
          {
            var pluginInstances = InstanceMap.Values;

            foreach (var pluginInstance in pluginInstances)
            {
              if (pluginInstance.IsStopping)
                continue;

              pluginInstance.Process.Refresh();

              if (pluginInstance.Process.HasExited)
                OnPluginStopped(pluginInstance);
            }
          }
          catch (Exception ex)
          {
            LogTo.Error(ex, "An exception occured while monitoring plugins");
          }

          sleepCounter = 0;
        }

        sleepCounter++;
        Thread.Sleep(250);
      }
    }

    private async Task StartPlugin(PluginPackage<PluginMetadata> pluginPackage)
    {
      bool   isDev     = pluginPackage.Metadata.IsDevelopment;
      string pluginLog = isDev ? "development plugin" : "plugin";

      LogTo.Information($"Starting {pluginLog} {pluginPackage.Id}");

      var startInfo = StartInfoMap[pluginPackage.Id] = new PluginStartInfo(pluginPackage);

      var smaProcId     = System.Diagnostics.Process.GetCurrentProcess().Id;
      var devSwitch     = isDev ? "--development" : string.Empty;
      var pluginId      = pluginPackage.Id;
      var pluginHomeDir = pluginPackage.GetHomeDir();

      var pluginStartInfo = new ProcessStartInfo(
        SMAFileSystem.PluginHostExeFile.FullPath,
        $"{pluginId.Quotify()} {smaProcId} {IpcServerChannelName.Quotify()} {pluginHomeDir.FullPath.Quotify()} {devSwitch}");

      try
      {
        var proc = System.Diagnostics.Process.Start(pluginStartInfo);

        if (proc == null)
        {
          LogTo.Warning($"Failed to start process for {pluginLog} {pluginId}");
          return;
        }

        if (await startInfo.ConnectedEvent.WaitAsync(PluginConnectTimeout))
          return;

        LogTo.Warning(
          $"{pluginLog.CapitalizeFirst()} {pluginId} failed to connect in {PluginConnectTimeout}ms. Attempting to kill process");

        proc.Refresh();

        if (proc.HasExited)
        {
          LogTo.Warning($"{pluginLog.CapitalizeFirst()} {pluginId} has already exited");
          return;
        }

        proc.Kill();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"An error occured while starting {pluginLog} {pluginId}");
      }
    }

    private async Task StopPlugin(PluginInstance pluginInstance)
    {
      bool   isDev     = pluginInstance.Metadata.IsDevelopment;
      string pluginLog = isDev ? "development plugin" : "plugin";
      LogTo.Information($"Stopping {pluginLog} {pluginInstance.Metadata.PackageName}.");

      try
      {
        try
        {
          pluginInstance.IsStopping = true;
          pluginInstance.Plugin.Dispose();

          if (await Task.Run(() => pluginInstance.Process.WaitForExit(PluginStopTimeout)))
            return;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, $"An exception occured while gracefully stopping plugin {pluginInstance.Metadata.PackageName}.");
        }

        try
        {
          pluginInstance.Process.Refresh();

          if (pluginInstance.Process.HasExited)
            return;

          LogTo.Warning(
            $"Plugin {pluginInstance.Metadata.PackageName} didn't shut down gracefully after {PluginStopTimeout}ms. Attempting to kill process");

          pluginInstance.Process.Kill();
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, $"An exception occured while killing plugin {pluginInstance.Metadata.PackageName}");
        }
      }
      finally
      {
        OnPluginStopped(pluginInstance);
      }
    }

    #endregion
  }
}
