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
// Created On:   2018/06/01 14:11
// Modified On:  2019/01/16 15:03
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Principal;
using System.Threading;
using Anotar.Serilog;
using EasyHook;
using Process.NET;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo.SuperMemo17;
using SuperMemoAssistant.Sys.Exceptions;

namespace SuperMemoAssistant.SuperMemo.Hooks
{
  public class SMHookEngine : SMHookCallback
  {
    #region Constants & Statics

#if DEBUG
    public const int WaitTimeout = 300000;
#else
    public const int WaitTimeout = 3000;
#endif

    public static SMHookEngine Instance { get; } = new SMHookEngine();

    #endregion




    #region Properties & Fields - Non-Public

    protected bool           HookSuccess   { get; private set; }
    protected Exception      HookException { get; private set; }
    protected AutoResetEvent HookInitEvent { get; set; }
    protected AutoResetEvent SMAInitEvent  { get; set; }

    protected IpcServerChannel ServerChannel     { get; set; }
    protected ISMHookSystem    SystemCallback    { get; private set; }
    protected List<string>     IOTargetFilePaths { get; set; }         = new List<string>();
    protected List<ISMHookIO>  IOCallbacks       { get; private set; } = new List<ISMHookIO>();

    #endregion




    #region Constructors

    //
    // Instance

    protected SMHookEngine() { }

    #endregion




    #region Methods Impl

    //
    // System callbacks

    public override bool OnHookInstalled(bool      success,
                                         Exception hookEx = null)
    {
      if (hookEx != null)
        LogTo.Error(hookEx,
                    "InjectionLib threw an error during initialization.");

      if (HookInitEvent == null)
        return false;

      try
      {
        HookSuccess   = success;
        HookException = hookEx;

        return HookInitEvent.Set() && SMAInitEvent.WaitOne(WaitTimeout);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Failed to Signal InitEvent for Hook Install Success");

        return false;
      }
    }

    public override void KeepAlive() { }

    public override void Debug(string          msg,
                               params object[] args)
    {
      LogTo.Debug(msg,
                  args);
    }

    public override void OnException(Exception ex)
    {
      SystemCallback.OnException(ex);

      StopIPCServer();
    }

    public override void SetWndProcHookAddr(int addr)
    {
      SystemCallback.SetWndProcHookAddr(addr);
    }

    public override bool OnUserMessage(int wParam)
    {
      return SystemCallback.OnUserMessage(wParam);
    }

    public override void GetExecutionParameters(out int       method,
                                                out dynamic[] parameters)
    {
      SystemCallback.GetExecutionParameters(out method,
                                            out parameters);
    }

    public override void SetExecutionResult(int result)
    {
      SystemCallback.SetExecutionResult(result);
    }

    public override Dictionary<string, int> GetPatternsHintAddresses()
    {
      return SMA.SMA.Instance.Config.PatternsHintAddresses;
    }

    public override void SetPatternsHintAddresses(Dictionary<string, int> hintAddrs)
    {
      SMA.SMA.Instance.Config.PatternsHintAddresses = hintAddrs;
      SMA.SMA.Instance.SaveConfig(false);
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
      for (int i = 0; i < IOCallbacks.Count; i++)
        IOCallbacks[i].OnFileCreate(filePath,
                                    fileHandle);
    }

    /// <inheritdoc />
    public override void OnFileSeek(IntPtr fileHandle,
                                    UInt32 position)
    {
      for (int i = 0; i < IOCallbacks.Count; i++)
        IOCallbacks[i].OnFileSeek(fileHandle,
                                  position);
    }

    /// <inheritdoc />
    public override void OnFileWrite(IntPtr fileHandle,
                                     Byte[] buffer,
                                     UInt32 count)
    {
      for (int i = 0; i < IOCallbacks.Count; i++)
        IOCallbacks[i].OnFileWrite(fileHandle,
                                   buffer,
                                   count);
    }

    /// <inheritdoc />
    public override void OnFileClose(IntPtr fileHandle)
    {
      for (int i = 0; i < IOCallbacks.Count; i++)
        IOCallbacks[i].OnFileClose(fileHandle);
    }

    #endregion




    #region Methods

    //
    // Core hook methods

    public IProcess CreateAndHook(
      SMCollection           collection,
      string                 binPath,
      ISMHookSystem          systemCallback,
      IEnumerable<ISMHookIO> ioCallbacks)
    {
      try
      {
        SystemCallback = systemCallback;
        IOCallbacks.AddRange(ioCallbacks);

        IOTargetFilePaths.AddRange(IOCallbacks.SelectMany(c => c.GetTargetFilePaths()));

        // Initialize event to non-Signaled
        HookInitEvent = new AutoResetEvent(false);
        SMAInitEvent  = new AutoResetEvent(false);

        HookSuccess   = false;
        HookException = null;

        // Start a new IPC server
        StartIPCServer();

        // Start SuperMemo application with given collection as parameter,
        // and immediatly install hooks
        RemoteHooking.CreateAndInject(
          binPath,
          collection.GetKnoFilePath().Quotify(),
          0,
          InjectionOptions.Default,
          SMAFileSystem.InjectionLibFile.FullPath,
          null,
          out var pId
        );

        // Wait for Signal from OnHookInstalled with timeout
        HookInitEvent.WaitOne(WaitTimeout);

        if (HookSuccess == false)
        {
          StopIPCServer();

          var ex = new HookException("Hook setup failed: " + HookException?.Message,
                                     HookException);
          HookException = null;

          throw ex;
        }

        return new ProcessSharp<SM17Natives>(
          pId,
          Process.NET.Memory.MemoryType.Remote,
          true,
          SMA.SMA.Instance.Config.PatternsHintAddresses);
      }
      finally
      {
        HookInitEvent = null;
      }
    }

    public void CleanupHooks()
    {
      StopIPCServer();

      SystemCallback = null;
      IOCallbacks.Clear();
      IOTargetFilePaths.Clear();
    }

    public void SignalWakeUp()
    {
      SMAInitEvent.Set();
    }

    private void StartIPCServer()
    {
      if (ServerChannel != null)
        throw new InvalidOperationException("IPC Server already started");

      string channelName = HookConst.ChannelName;

      // TODO: Switch to Duplex (get callback)
      ServerChannel = RemoteHooking.IpcCreateServer<SMHookCallback>(
        ref channelName,
        WellKnownObjectMode.Singleton,
        this,
        WellKnownSidType.WorldSid
      );
    }

    private void StopIPCServer()
    {
      ServerChannel?.StopListening(null);
      ServerChannel = null;
    }

    #endregion
  }
}
