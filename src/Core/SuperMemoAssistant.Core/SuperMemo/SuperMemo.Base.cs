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
// Modified On:  2019/02/24 23:11
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using Process.NET;
using Process.NET.Assembly;
using Process.NET.Memory;
using Process.NET.Utilities;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SMA.Hooks;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.SuperMemo.SuperMemo17;

namespace SuperMemoAssistant.SuperMemo
{
  /// <summary>Convenience class that implements helpers</summary>
  public abstract class SuperMemoBase
    : IDisposable,
      ISuperMemo,
      ISMAHookSystem
  {
    #region Properties & Fields - Non-Public

    private readonly   string           _binPath;
    protected readonly ExecutionContext _execCtxt = new ExecutionContext();

    private IPointer _ignoreUserConfirmationPtr;

    private int _wndProcHookAddr;

    #endregion




    #region Constructors

    protected SuperMemoBase(SMCollection collection,
                            string       binPath)
    {
      Collection = collection;
      _binPath   = binPath;
    }

    public virtual void Dispose()
    {
      _ignoreUserConfirmationPtr = null;

      SMProcess.Native.Exited -= OnSMExited;

      try
      {
        SMHookEngine.Instance.CleanupHooks();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Failed to cleanup SMHookEngine");
      }

      try
      {
        SMA.SMA.Instance.OnSMStopped().Wait();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "An exception occured in one of OnSMStoppedEvent handlers");
      }
    }

    #endregion




    #region Properties & Fields - Public

    public ManualResetEvent           MainThreadReady { get; } = new ManualResetEvent(false);
    public IProcess                   SMProcess       { get; private set; }
    public System.Diagnostics.Process NativeProcess   => SMProcess.Native;

    #endregion




    #region Properties Impl - Public

    public SMCollection Collection { get; }
    public bool IgnoreUserConfirmation
    {
      get => _ignoreUserConfirmationPtr.Read<bool>();
      set => _ignoreUserConfirmationPtr.Write<bool>(0,
                                                    value);
    }
    public ISuperMemoRegistry Registry => SuperMemoRegistry.Instance;
    public ISuperMemoUI       UI       => SuperMemoUI.Instance;

    #endregion




    #region Methods Impl

    //
    // SM Hook

    public virtual void OnException(Exception ex)
    {
      // TODO: Notify ?
      LogTo.Error(ex,
                  "Exception caught in InjectLib.");
    }

    /// <inheritdoc />
    public void SetWndProcHookAddr(int addr)
    {
      _wndProcHookAddr = addr;
    }

    /// <param name="wParam"></param>
    public bool OnUserMessage(int wParam)
    {
      switch ((InjectLibMessages)wParam)
      {
        case InjectLibMessages.ExecuteOnMainThread:
          //MainThreadReady.Set();

          return true;
      }

      return false;
    }

    public void GetExecutionParameters(out int       method,
                                       out dynamic[] parameters)
    {
      method     = (int)_execCtxt.ExecutionMethod;
      parameters = _execCtxt.ExecutionParameters;
    }

    public void SetExecutionResult(int result)
    {
      _execCtxt.ExecutionResult = result;
      MainThreadReady.Set();
    }

    public Dictionary<string, int> GetPatternsHintAddresses()
    {
      throw new NotImplementedException();
    }

    public void SetPatternsHintAddresses(Dictionary<string, int> hintAddrs)
    {
      throw new NotImplementedException();
    }

    #endregion




    #region Methods

    // TODO: Not thread safe & ugly overall...
    public int ExecuteOnMainThread(NativeMethod     method,
                                   params dynamic[] parameters)
    {
      MainThreadReady.Reset();

      _execCtxt.ExecutionResult     = 0;
      _execCtxt.ExecutionMethod     = method;
      _execCtxt.ExecutionParameters = parameters;

      var smMain = SMProcess.Memory.Read<int>(SM17Natives.TSMMain.InstancePtr);
      var handle = SMProcess.Memory.Read<IntPtr>(new IntPtr(smMain + SM17Natives.TControl.HandleOffset));

      var restoreWndProcAddr = SM17Natives.TApplication.TApplicationOnMessagePtr.Read<int>(SMProcess.Memory);
      SM17Natives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                   _wndProcHookAddr);

      WindowHelper.PostMessage(handle,
                               2345,
                               new IntPtr((int)InjectLibMessages.ExecuteOnMainThread),
                               new IntPtr(0));

      SMA.SMA.Instance.SMMgmt.MainThreadReady.WaitOne(AssemblyFactory.ExecutionTimeout);

      SM17Natives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                   restoreWndProcAddr);

      _execCtxt.ExecutionParameters = null;

      return _execCtxt.ExecutionResult;
    }

    //
    // SM-App Lifecycle

    public async Task Start()
    {
      await OnPreInit();

      SMProcess = await SMHookEngine.Instance.CreateAndHook(
        Collection,
        _binPath,
        this,
        GetIOCallbacks()
      );

      SMProcess.Native.Exited += OnSMExited;

      await OnPostInit();

      SMHookEngine.Instance.SignalWakeUp();
    }

    protected virtual Task OnPreInit()
    {
      return SMA.SMA.Instance.OnSMStarting();
    }

    protected virtual Task OnPostInit()
    {
      _ignoreUserConfirmationPtr = SMProcess[SM17Natives.Globals.IgnoreUserConfirmationPtr];

      return SMA.SMA.Instance.OnSMStarted();
    }

    protected virtual void OnSMExited(object    called,
                                      EventArgs args)
    {
      Dispose();
    }

    #endregion




    #region Methods Abs

    protected abstract IEnumerable<ISMAHookIO> GetIOCallbacks();


    //
    // ISuperMemo Methods

    public abstract SMAppVersion AppVersion { get; }

    #endregion




    protected class ExecutionContext
    {
      #region Properties & Fields - Public

      public NativeMethod ExecutionMethod     { get; set; }
      public dynamic[]    ExecutionParameters { get; set; }
      public int          ExecutionResult     { get; set; }

      #endregion
    }
  }
}
