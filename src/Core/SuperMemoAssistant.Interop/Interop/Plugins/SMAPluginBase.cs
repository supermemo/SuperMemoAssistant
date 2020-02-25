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
// Modified On:  2020/02/25 15:26
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Serilog;
using PluginManager.Interop.Plugins;
using Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.IO.Logger;
using SuperMemoAssistant.Sys.Remoting;

namespace SuperMemoAssistant.Interop.Plugins
{
  public abstract class SMAPluginBase<TPlugin> : PluginBase<TPlugin, ISMAPlugin, ISuperMemoAssistant>, ISMAPlugin
    where TPlugin : SMAPluginBase<TPlugin>
  {
    #region Constructors

    static SMAPluginBase()
    {
      RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
    }

    protected SMAPluginBase(DebuggerAttachStrategy debuggerAttachStrategy = DebuggerAttachStrategy.Never)
      : base(RemotingServicesEx.GenerateIpcServerChannelName())
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

      try
      {
        // Required for logging
        Svc.App                 = CreateApplication();
        Svc.SharedConfiguration = new ConfigurationService(SMAFileSystem.SharedConfigDir);

        Svc.Logger = LoggerFactory.Create(AssemblyName, Svc.SharedConfiguration, ConfigureLogger);
        ReloadAnotarLogger();

        Svc.KeyboardHotKey       = KeyboardHookService.Instance;
        Svc.KeyboardHotKeyLegacy = KeyboardHotKeyService.Instance;
        Svc.Configuration        = new PluginConfigurationService(this);
        Svc.HotKeyManager        = HotKeyManager.Instance.Initialize(Svc.Configuration, Svc.KeyboardHotKey);

        LogTo.Debug($"Plugin {AssemblyName} initialized");
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"Exception while initializing {GetType().Name}");
        throw;
      }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
      try
      {
        KeyboardHotKeyService.Instance.Dispose();

        Svc.App?.Dispatcher.InvokeShutdown();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Plugin threw an exception while disposing.");
      }

      Svc.Logger.Shutdown();

      base.Dispose();
    }

    #endregion




    #region Properties & Fields - Public

    public ISuperMemoAssistant SMA => Service;

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public virtual bool HasSettings => false;

    #endregion




    #region Methods Impl

    protected override void LogError(Exception ex, string message)
    {
      LogTo.Error(ex, message);
    }

    protected override void LogInformation(string message)
    {
      LogTo.Information(message);
    }

    /// <inheritdoc />
    [LogToErrorOnException]
    public override void OnInjected()
    {
      if (SessionGuid == Guid.Empty)
        throw new NullReferenceException($"{nameof(SessionGuid)} is empty");

      if (SMA == null)
        throw new NullReferenceException($"{nameof(SMA)} is null");

      if (PluginMgr == null)
        throw new NullReferenceException($"{nameof(PluginMgr)} is null");

      Svc.Plugin                  = this;
      Svc.SMA                     = SMA;
      Svc.CollectionConfiguration = new CollectionConfigurationService(Svc.SM.Collection, this);

      PluginInit();
    }

    /// <inheritdoc />
    public virtual RemoteTask<object> OnMessage(PluginMessage msg, params object[] parameters)
    {
      switch (msg)
      {
        case PluginMessage.OnLoggerConfigUpdated:
          return OnLoggerConfigUpdated();

        default:
          LogTo.Debug($"Received unknown message {msg}. Is plugin up to date ?");
          break;
      }

      return Task.FromResult<object>(null);
    }

    /// <inheritdoc />
    public virtual void ShowSettings()
    {
      // Ignored -- override for desired behavior
    }

    #endregion




    #region Methods

    private async Task<object> OnLoggerConfigUpdated()
    {
      try
      {
        await Svc.Logger.ReloadConfigFromFile(Svc.SharedConfiguration);

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Warning(ex, "Exception caught while reloading logger config");
        return false;
      }
    }

    [Conditional("DEBUG")]
    [Conditional("DEBUG_IN_PROD")]
    private void AttachDebuggerIfDebug()
    {
      Debugger.Launch();
    }

    // See https://github.com/Fody/Anotar/issues/114
    private void ReloadAnotarLogger()
    {
      // SMAPluginBase's logger
      Logger.ReloadAnotarLogger<SMAPluginBase<TPlugin>>();

      // TPlugin's logger
      Logger.ReloadAnotarLogger<TPlugin>();
    }

    protected virtual Application CreateApplication()
    {
      return new PluginApp();
    }

    protected virtual LoggerConfiguration ConfigureLogger(LoggerConfiguration loggerConfiguration)
    {
      return loggerConfiguration;
    }

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
