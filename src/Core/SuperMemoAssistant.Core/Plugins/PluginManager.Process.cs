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
// Created On:   2019/01/26 03:52
// Modified On:  2019/01/26 06:02
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
        if (sleepCounter > 15)
        {
          try
          {
            var pluginInstances = InstanceMap.Values;

            foreach (var pluginInstance in pluginInstances)
            {
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
        Thread.Sleep(100);
      }
    }

    private async Task StartPlugin(PluginPackage<PluginMetadata> pluginPackage)
    {
      LogTo.Information($"Stating plugin {pluginPackage.Id}");

      var startInfo = StartInfoMap[pluginPackage.Id] = new PluginStartInfo(pluginPackage);

      var smaProcId     = System.Diagnostics.Process.GetCurrentProcess().Id;
      var pluginId      = pluginPackage.Id;
      var pluginHomeDir = SMAFileSystem.PluginHomeDir.Combine(pluginId);

      var pluginStartInfo = new ProcessStartInfo(
        SMAFileSystem.GetPluginHostExeFile().FullPath,
        $"{pluginId.Quotify()} {smaProcId} {IpcServerChannelName.Quotify()}")
      {
        WorkingDirectory = pluginHomeDir.FullPath
      };

      try
      {
        var proc = System.Diagnostics.Process.Start(pluginStartInfo);

        if (proc == null)
        {
          LogTo.Warning($"Failed to start process for plugin {pluginId}");
          return;
        }

        if (await startInfo.ConnectedEvent.WaitAsync(PluginConnectTimeout))
          return;

        LogTo.Warning($"Plugin {pluginId} failed to connect in {PluginConnectTimeout}ms. Attempting to kill process");

        proc.Refresh();

        if (proc.HasExited)
        {
          LogTo.Warning($"Plugin {pluginId} has already exited");
          return;
        }

        proc.Kill();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"An error occured while starting plugin {pluginId}");
      }
    }

    private async Task StopPlugin(PluginInstance pluginInstance)
    {
      try
      {
        try
        {
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
        InstanceMap.TryRemove(pluginInstance.Plugin, out _);
      }
    }

    #endregion
  }
}
