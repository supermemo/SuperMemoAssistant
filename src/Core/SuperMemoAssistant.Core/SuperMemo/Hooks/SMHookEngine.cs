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

#endregion




namespace SuperMemoAssistant.SuperMemo.Hooks
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.Remoting.Channels.Ipc;
  using System.Threading;
  using System.Threading.Tasks;
  using Anotar.Serilog;
  using EasyHook;
  using Extensions;
  using Interop;
  using Interop.SuperMemo.Core;
  using Natives;
  using Nito.AsyncEx;
  using PluginManager.Interop.Sys;
  using Process.NET;
  using SMA;
  using SMA.Hooks.Services;
  using Sys.Exceptions;

  public sealed partial class SMHookEngine
    : PerpetualMarshalByRefObject, ISMAHook, IDisposable
  {
    #region Constants & Statics

#if DEBUG
    public const int WaitTimeout = 300000;
    private string _injectLibChannelName;
#else
    public const int WaitTimeout = 5000;
#endif

    #endregion




    #region Properties & Fields - Non-Public

    private bool                  HookSuccess   { get; set; }
    private Exception             HookException { get; set; }
    private AsyncManualResetEvent HookInitEvent { get; } = new AsyncManualResetEvent(false);
    private AutoResetEvent        SMAInitEvent  { get; } = new AutoResetEvent(false);

    private IpcServerChannel ServerChannel { get; set; }

    private ISMInject _injectLib;

    #endregion




    #region Constructors

    //
    // Instance

    public SMHookEngine()
    {
      Core.Hook = this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      SMAInitEvent?.Dispose();
    }

    #endregion




    #region Methods Impl

    public bool OnHookInstalled(string    injectLibChannelName,
                                Exception hookEx)
    {
      if (hookEx != null)
        OnException(hookEx);

      var success = hookEx == null;

      try
      {
        LogTo.Debug("Injected lib signal, success: {Success}", success);

        _injectLibChannelName = injectLibChannelName;

        HookSuccess   = success;
        HookException = hookEx;

        HookInitEvent.Set();

        return SMAInitEvent.WaitOne(WaitTimeout);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Failed to Signal InitEvent for Hook Install Success");

        return false;
      }
    }

    #endregion




    #region Methods

    //
    // Core hook methods

    public async Task<ProcessSharp<SMNatives>> CreateAndHookAsync(
      SMCollection            collection,
      string                  binPath,
      IEnumerable<ISMAHookIO> ioCallbacks,
      NativeData              nativeData)
    {
      LogTo.Debug("Starting and injecting SuperMemo");

      IOCallbacks.AddRange(ioCallbacks);

      IOTargetFilePaths.AddRange(IOCallbacks.SelectMany(c => c.GetTargetFilePaths()));

      HookSuccess   = false;
      HookException = null;

      // Start a new IPC server
      var channelName = StartIPCServer();

      // Start SuperMemo application with given collection as parameter,
      // and immediately install hooks
      int pId = -1;

      try
      {
        RemoteHooking.CreateAndInject(
          binPath,
          collection.GetKnoFilePath().Quotify(),
          0,
          InjectionOptions.Default,
          SMAFileSystem.InjectionLibFile.FullPath,
          null,
          out pId,
          channelName,
          nativeData
        );
      }
      catch (ArgumentException ex)
      {
        LogTo.Warning(ex, "Failed to start and inject SuperMemo. Command line: '{BinPath} {V}'", binPath,
                      collection.GetKnoFilePath().Quotify());
      }

      LogTo.Debug("Waiting for signal from Injected library");

      // Wait for Signal from OnHookInstalled with timeout
      await HookInitEvent.WaitAsync(WaitTimeout).ConfigureAwait(false);

      if (HookSuccess)
      {
        try
        {
          _injectLib = RemotingServicesEx.ConnectToIpcServer<ISMInject>(_injectLibChannelName);

          // Sanity check
          _ = _injectLib.ToString();
        }
        catch (Exception ex)
        {
          HookSuccess   = false;
          HookException = ex;
        }
      }

      if (HookSuccess == false)
      {
        LogTo.Debug("Hook failed, aborting");

        StopIPCServer();

        var ex = new HookException("Hook setup failed: " + HookException?.Message,
                                   HookException);
        HookException = null;

        throw ex;
      }

      LogTo.Debug("SuperMemo started and injected, pId: {PId}", pId);

      return new ProcessSharp<SMNatives>(
        pId,
        Process.NET.Memory.MemoryType.Remote,
        true,
        Core.CoreConfig.SuperMemo.PatternsHintAddresses,
        nativeData);
    }

    public void CleanupHooks()
    {
      StopIPCServer();

      IOCallbacks.Clear();
      IOTargetFilePaths.Clear();
    }

    public void SignalWakeUp()
    {
      SMAInitEvent.Set();
    }

    private string StartIPCServer()
    {
      if (ServerChannel != null)
        throw new InvalidOperationException("IPC Server already started");

      var channelName = RemotingServicesEx.GenerateIpcServerChannelName();

      // TODO: Switch to Duplex (get callback)
      ServerChannel = RemotingServicesEx.CreateIpcServer<ISMAHook, SMHookEngine>(
        this,
        channelName
      );

      return channelName;
    }

    private void StopIPCServer()
    {
      ServerChannel?.StopListening(null);
      ServerChannel = null;
    }

    #endregion
  }
}
