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
// Created On:   2019/09/03 18:08
// Modified On:  2020/01/11 18:56
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using EasyHook;
using Nito.AsyncEx;
using Process.NET;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SMA.Hooks;
using SuperMemoAssistant.SuperMemo.Natives;
using SuperMemoAssistant.Sys.Exceptions;

namespace SuperMemoAssistant.SuperMemo.Hooks
{
  public partial class SMHookEngine : SMAHookCallback
  {
    #region Constants & Statics

#if DEBUG
    public const int WaitTimeout = 300000;
#else
    public const int WaitTimeout = 5000;
#endif

    #endregion




    #region Properties & Fields - Non-Public

    protected bool                  HookSuccess   { get; private set; }
    protected Exception             HookException { get; private set; }
    protected AsyncManualResetEvent HookInitEvent { get; } = new AsyncManualResetEvent(false);
    protected AutoResetEvent        SMAInitEvent  { get; } = new AutoResetEvent(false);

    protected IpcServerChannel ServerChannel { get; set; }

    protected List<string>     IOTargetFilePaths { get; } = new List<string>();
    protected List<ISMAHookIO> IOCallbacks       { get; } = new List<ISMAHookIO>();

    #endregion




    #region Constructors

    //
    // Instance

    public SMHookEngine()
    {
      Core.Hook = this;
    }

    #endregion




    #region Methods Impl

    //
    // System callbacks

    public override bool OnHookInstalled(bool      success,
                                         Exception hookEx = null)
    {
      if (hookEx != null)
        OnException(hookEx);

      try
      {
        LogTo.Debug($"Injected lib signal, success: {success}");

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

    public override void KeepAlive() { }

    public override void Debug(string          msg,
                               params object[] args)
    {
      LogTo.Debug(msg, args);
    }


    //
    // IO callbacks

    /// <inheritdoc />
    public override IEnumerable<string> GetTargetFilePaths()
    {
      return IOTargetFilePaths;
    }

    /// <inheritdoc />
    public override void OnFileCreate(string filePath,
                                      IntPtr fileHandle)
    {
      foreach (var callback in IOCallbacks)
        callback.OnFileCreate(filePath,
                              fileHandle);
    }

    /// <inheritdoc />
    public override void OnFileSeek(IntPtr fileHandle,
                                    UInt32 position)
    {
      foreach (var callback in IOCallbacks)
        callback.OnFileSeek(fileHandle,
                            position);
    }

    /// <inheritdoc />
    public override void OnFileWrite(IntPtr fileHandle,
                                     Byte[] buffer,
                                     UInt32 count)
    {
      foreach (var callback in IOCallbacks)
        callback.OnFileWrite(fileHandle,
                             buffer,
                             count);
    }

    /// <inheritdoc />
    public override void OnFileClose(IntPtr fileHandle)
    {
      foreach (var callback in IOCallbacks)
        callback.OnFileClose(fileHandle);
    }

    #endregion




    #region Methods

    //
    // Core hook methods

    public async Task<ProcessSharp<SMNatives>> CreateAndHook(
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
        LogTo.Warning(ex, $"Failed to start and inject SuperMemo. Command line: '{binPath} {collection.GetKnoFilePath().Quotify()}'");
      }

      LogTo.Debug("Waiting for signal from Injected library");

      // Wait for Signal from OnHookInstalled with timeout
      await HookInitEvent.WaitAsync(WaitTimeout);

      if (HookSuccess == false)
      {
        LogTo.Debug("Hook failed, aborting");

        StopIPCServer();

        var ex = new HookException("Hook setup failed: " + HookException?.Message,
                                   HookException);
        HookException = null;

        throw ex;
      }

      LogTo.Debug($"SuperMemo started and injected, pId: {pId}");

      return new ProcessSharp<SMNatives>(
        pId,
        Process.NET.Memory.MemoryType.Remote,
        true,
        Core.SMA.CoreConfig.SuperMemo.PatternsHintAddresses,
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
      ServerChannel = RemotingServicesEx.CreateIpcServer<SMAHookCallback, SMHookEngine>(
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
