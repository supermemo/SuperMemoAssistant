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
// Created On:   2019/02/24 00:49
// Modified On:  2019/02/24 21:18
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo;

namespace SuperMemoAssistant.Plugins
{
  public partial class PluginManager
  {
    #region Methods Impl

    /// <inheritdoc />
    public bool GetAssembliesPathsForPlugin(Guid                    sessionGuid,
                                            out IEnumerable<string> pluginAssemblies,
                                            out IEnumerable<string> dependenciesAssemblies)
    {
      string pluginPackageName = "N/A";
      pluginAssemblies       = null;
      dependenciesAssemblies = null;

      try
      {
        var pluginInstance = _runningPluginMap[sessionGuid];

        if (pluginInstance == null)
        {
          LogTo.Warning($"Plugin {sessionGuid} unexpected for assembly request. Aborting");
          return false;
        }

        pluginPackageName = pluginInstance.Package.Id;

        LogTo.Information($"Fetching assemblies requested by plugin {pluginPackageName}");

        if (pluginInstance.Metadata.IsDevelopment)
          throw new InvalidOperationException($"Development plugin {pluginPackageName} cannot request assemblies paths");

        var pm = PackageManager;

        lock (pm)
        {
          var pluginPkg = pm.FindInstalledPluginById(pluginPackageName);

          if (pluginPkg == null)
            throw new InvalidOperationException($"Cannot find requested plugin package {pluginPackageName}");

          pm.GetInstalledPluginAssembliesFilePath(
            pluginPkg.Identity,
            out var tmpPluginAssemblies,
            out var tmpDependenciesAssemblies);

          pluginAssemblies       = tmpPluginAssemblies.Select(p => p.FullPath);
          dependenciesAssemblies = tmpDependenciesAssemblies.Select(p => p.FullPath);
        }

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"An exception occured while returning assemblies path for plugin {pluginPackageName}");

        return false;
      }
    }

    /// <inheritdoc />
    public ISuperMemoAssistant ConnectPlugin(string channel,
                                             Guid   sessionGuid)
    {
      string pluginAssemblyName = "N/A";

      try
      {
        var plugin = RemotingServicesEx.ConnectToIpcServer<ISMAPlugin>(channel);
        pluginAssemblyName = plugin.AssemblyName;

        // TODO: Make sure no double run
        var pluginInstance = _runningPluginMap.SafeGet(sessionGuid);

        if (pluginInstance == null)
        {
          LogTo.Warning($"Plugin {pluginAssemblyName} unexpected for connection. Aborting");
          return null;
        }
        
        using (pluginInstance.Lock.Lock())
          OnPluginConnected(pluginInstance, plugin);

        /*pluginInst.Process.Exited += (o,
                                      ev) => OnPluginStopped(pluginInst, true);*/

        return SMA.SMA.Instance;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"An exception occured while connecting plugin {pluginAssemblyName}");

        return null;
      }
    }

    /// <inheritdoc />
    public string GetService(string remoteInterfaceType)
    {
      return _interfaceChannelMap.SafeGet(remoteInterfaceType);
    }

    /// <inheritdoc />
    public IDisposable RegisterService(Guid   sessionGuid,
                                       string remoteServiceType,
                                       string channelName)
    {
      var pluginInst = _runningPluginMap.SafeGet(sessionGuid);

      if (pluginInst == null)
        throw new ArgumentException("Invalid plugin");

      pluginInst.InterfaceChannelMap[remoteServiceType] = channelName;
      _interfaceChannelMap[remoteServiceType]           = channelName;

      Task.Run(() => NotifyServicePublished(remoteServiceType));

      return new PluginChannelDisposer(this, remoteServiceType, sessionGuid);
    }

    #endregion




    #region Methods

    public void UnregisterChannelType(string remoteServiceType,
                                      Guid   sessionGuid)
    {
      var pluginInst = _runningPluginMap.SafeGet(sessionGuid);

      if (pluginInst == null)
        throw new ArgumentException("Invalid plugin");

      pluginInst.InterfaceChannelMap.TryRemove(remoteServiceType, out _);
      _interfaceChannelMap.TryRemove(remoteServiceType, out _);

      Task.Run(() => NotifyServiceRevoked(remoteServiceType));
    }

    private void NotifyServicePublished(string remoteServiceType)
    {
      foreach (var pluginKeyValue in _runningPluginMap.Values)
      {
        var plugin     = pluginKeyValue.Plugin;
        var pluginName = pluginKeyValue.Metadata.PackageName;

        try
        {
          plugin.OnServicePublished(remoteServiceType);
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, $"Exception while notifying plugin {pluginName} of published service {remoteServiceType}");
        }
      }
    }

    private void NotifyServiceRevoked(string remoteServiceType)
    {
      foreach (var pluginKeyValue in _runningPluginMap.Values)
      {
        var plugin     = pluginKeyValue.Plugin;
        var pluginName = pluginKeyValue.Metadata.PackageName;

        try
        {
          plugin.OnServiceRevoked(remoteServiceType);
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, $"Exception while notifying plugin {pluginName} of revoked service {remoteServiceType}");
        }
      }
    }

    #endregion
  }
}
