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
// Modified On:  2019/02/24 21:21
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Services.IO;
using SuperMemoAssistant.Services.IO.Devices;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.Interop.Plugins
{
  public abstract class SMAPluginBase<TPlugin> : PerpetualMarshalByRefObject, ISMAPlugin
    where TPlugin : SMAPluginBase<TPlugin>
  {
    #region Properties & Fields - Non-Public

    private readonly string _channelName;


    private ConcurrentDictionary<string, (IpcServerChannel ipcServer, IDisposable disposable)> RegisteredServicesMap { get; } =
      new ConcurrentDictionary<string, (IpcServerChannel, IDisposable)>();
    private ConcurrentDictionary<string, object> ConsumedServiceMap { get; } = new ConcurrentDictionary<string, object>();

    #endregion




    #region Constructors

    protected SMAPluginBase(bool                   startApplication       = true,
                            DebuggerAttachStrategy debuggerAttachStrategy = DebuggerAttachStrategy.Never)
    {
      switch (debuggerAttachStrategy)
      {
        case DebuggerAttachStrategy.Always:
          Debugger.Launch();
          break;

        case DebuggerAttachStrategy.InDebugConfiguration:
          AttachDebuggerIfDebug();
          break;
      }

      Logger.Instance.Initialize(AssemblyName, ConfigureLogger);

      AppDomain.CurrentDomain.UnhandledException += (_, e) => LogException((Exception)e.ExceptionObject, e.IsTerminating);
      
      if (startApplication)
      {
        App = new PluginApp();
        App.DispatcherUnhandledException += (_, e) =>
        {
#if !DEBUG
          e.Handled = true;
#endif
          LogException(e.Exception, !e.Handled);
        };
      }

      // Create Plugin's IPC Server
      _channelName = RemotingServicesEx.GenerateIpcServerChannelName();
      RemotingServicesEx.CreateIpcServer<ISMAPlugin, SMAPluginBase<TPlugin>>(
        this,
        _channelName);
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
      //RevokeServices();

      try
      {
        //_ipcServer.StopListening(null);

        KeyboardHotKeyService.Instance.Dispose();

        App?.Dispatcher.InvokeShutdown();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Plugin threw an exception while disposing.");
      }

      Logger.Instance.Shutdown();

      // TODO: Improve this
      Task.Factory.StartNew(() =>
      {
        Task.Yield();
        Environment.Exit(0);
      });
    }

    #endregion




    #region Properties & Fields - Public

    public Application App { get; set; }

    public ISuperMemoAssistant SMA          { get; set; }
    public ISMAPluginManager   SMAPluginMgr { get; set; }
    public Guid                SessionGuid  { get; set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public string AssemblyName => AssemblyEx.GetAssemblyName(GetType());
    /// <inheritdoc />
    public string AssemblyVersion => AssemblyEx.GetAssemblyVersion(GetType());
    /// <inheritdoc />
    public string ChannelName => _channelName;
    /// <inheritdoc />
    public virtual bool HasSettings => false;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public void OnInjected()
    {
      if (SessionGuid == Guid.Empty)
        throw new NullReferenceException($"{nameof(SessionGuid)} is empty");

      if (SMA == null)
        throw new NullReferenceException($"{nameof(SMA)} is null");

      if (SMAPluginMgr == null)
        throw new NullReferenceException($"{nameof(SMAPluginMgr)} is null");

      Svc.Plugin               = this;
      Svc.SMA                  = SMA;
      Svc.KeyboardHotKey       = KeyboardHookService.Instance;
      Svc.KeyboardHotKeyLegacy = KeyboardHotKeyService.Instance;
      Svc.Configuration        = new PluginConfigurationService(this);

      PluginInit();
    }

    /// <inheritdoc />
    public virtual void OnServicePublished(string interfaceTypeName)
    {
      // Ignored -- override for desired behavior
    }

    /// <inheritdoc />
    public virtual void OnServiceRevoked(string interfaceTypeName)
    {
      ConsumedServiceMap.TryRemove(interfaceTypeName, out _);
    }

    /// <inheritdoc />
    public virtual void ShowSettings()
    {
      // Ignored -- override for desired behavior
    }

    #endregion




    #region Methods

    private void LogException(Exception ex,
                              bool      terminating)
    {
      LogTo.Error(ex, $"Unhandled exception, terminating: {terminating}");
    }

    [Conditional("DEBUG")]
    [Conditional("DEBUG_IN_PROD")]
    private void AttachDebuggerIfDebug()
    {
      Debugger.Launch();
    }

    protected virtual LoggerConfiguration ConfigureLogger(LoggerConfiguration loggerConfiguration)
    {
      return loggerConfiguration;
    }

    public IService GetService<IService>()
      where IService : class
    {
      var svcType = typeof(IService);

      if (svcType.IsInterface == false)
        throw new ArgumentException($"{nameof(IService)} must be an interface");

      var svcTypeName = svcType.FullName;

      if (svcTypeName == null)
        throw new ArgumentException("Invalid type");

      if (ConsumedServiceMap.ContainsKey(svcTypeName))
        return (IService)ConsumedServiceMap[svcTypeName];

      var channelName = SMAPluginMgr.GetService(svcTypeName);

      if (string.IsNullOrWhiteSpace(channelName))
        return null;

      try
      {
        var svc = RemotingServicesEx.ConnectToIpcServer<IService>(channelName);

        ConsumedServiceMap[svcTypeName] = svc;

        return svc;
      }
      catch (RemotingException ex)
      {
        LogTo.Error(ex, $"Failed to acquire remoting object for published service {svcTypeName}");
        return null;
      }
    }

    public void PublishService<IService, TService>(TService service)
      where IService : class
      where TService : PerpetualMarshalByRefObject, IService
    {
      var svcType = typeof(IService);

      if (svcType.IsInterface == false)
        throw new ArgumentException($"{nameof(IService)} must be an interface");

      var svcTypeName = svcType.FullName;

      if (svcTypeName == null)
        throw new ArgumentException("Invalid type");

      if (RegisteredServicesMap.ContainsKey(svcTypeName))
        throw new ArgumentException($"Service {svcTypeName} already registered");

      LogTo.Information($"Publishing service {svcTypeName}");

      var channelName = RemotingServicesEx.GenerateIpcServerChannelName();
      var ipcServer   = RemotingServicesEx.CreateIpcServer<IService, TService>(service, channelName);

      var unregisterObj = SMAPluginMgr.RegisterService(SessionGuid, svcTypeName, channelName);

      RegisteredServicesMap[svcTypeName] = (ipcServer, unregisterObj);
    }

    public bool RevokeService<IService>()
      where IService : class
    {
      return RevokeService(typeof(IService));
    }

    public bool RevokeService(Type svcType)
    {
      if (svcType.IsInterface == false)
        throw new ArgumentException("Service type must be an interface");

      var svcTypeName = svcType.FullName;

      if (svcTypeName == null)
        throw new ArgumentException("Invalid type");

      if (RegisteredServicesMap.Remove(svcTypeName, out var svcData) == false)
        return false;

      LogTo.Information($"Revoking service {svcTypeName}");

      svcData.disposable.Dispose();
      svcData.ipcServer.StopListening(null);

      return true;
    }

    public void RevokeServices()
    {
      foreach (var svcKeyValue in RegisteredServicesMap)
      {
        var svcType = svcKeyValue.Key;
        var svcData = svcKeyValue.Value;

        LogTo.Information($"Revoking service {svcType}");

        try
        {
          svcData.ipcServer.StopListening(null);
          svcData.disposable.Dispose();
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, $"Exception while stopping published service {svcType}");
        }
      }
    }

    #endregion




    #region Methods Abs

    protected abstract void PluginInit();

    /// <inheritdoc />
    public abstract string Name { get; }

    #endregion




    #region Enums

    protected enum DebuggerAttachStrategy
    {
      Never,
      InDebugConfiguration,
      Always
    }

    #endregion
  }
}
