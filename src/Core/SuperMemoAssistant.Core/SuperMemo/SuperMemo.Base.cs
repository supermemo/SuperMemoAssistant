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
// Created On:   2018/05/12 18:26
// Modified On:  2019/01/19 05:04
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Threading;
using Anotar.Serilog;
using Process.NET;
using Process.NET.Assembly;
using Process.NET.Memory;
using Process.NET.Utilities;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo.Hooks;
using SuperMemoAssistant.SuperMemo.SuperMemo17;

namespace SuperMemoAssistant.SuperMemo
{
  /// <summary>Convenience class that implements helpers</summary>
  public abstract class SuperMemoBase
    : IDisposable,
      ISuperMemo,
      ISMHookSystem
  {
    #region Properties & Fields - Non-Public

    private IPointer IgnoreUserConfirmationPtr { get; set; }


    protected ExecutionContext ExecCtxt { get; } = new ExecutionContext();

    private int WndProcHookAddr { get; set; }

    #endregion




    #region Constructors

    protected SuperMemoBase(SMCollection collection,
                            string       binPath)
    {
      Collection = collection;

      OnPreInit();

      SMProcess = SMHookEngine.Instance.CreateAndHook(
        collection,
        binPath,
        this,
        GetIOCallbacks()
      );

      SMProcess.Native.Exited += OnSMExited;

      OnPostInit();

      SMHookEngine.Instance.SignalWakeUp();
    }

    public virtual void Dispose()
    {
      IgnoreUserConfirmationPtr = null;

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
        OnSMStoppedEvent?.Invoke(this,
                                 new SMProcessArgs(this,
                                                   NativeProcess));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "An exception occured in one of OnSMStoppedEvent handlers");
      }
    }

    #endregion




    #region Properties & Fields - Public

    public ManualResetEvent MainThreadReady { get; } = new ManualResetEvent(false);
    public IProcess         SMProcess       { get; }

    #endregion




    #region Properties Impl - Public

    public SMCollection               Collection    { get; }
    public System.Diagnostics.Process NativeProcess => SMProcess.Native;
    public bool IgnoreUserConfirmation
    {
      get => IgnoreUserConfirmationPtr.Read<bool>();
      set => IgnoreUserConfirmationPtr.Write<bool>(0,
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
      WndProcHookAddr = addr;
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
      method     = (int)ExecCtxt.ExecutionMethod;
      parameters = ExecCtxt.ExecutionParameters;
    }

    public void SetExecutionResult(int result)
    {
      ExecCtxt.ExecutionResult = result;
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

      ExecCtxt.ExecutionResult     = 0;
      ExecCtxt.ExecutionMethod     = method;
      ExecCtxt.ExecutionParameters = parameters;

      var smMain = SMProcess.Memory.Read<int>(SM17Natives.TSMMain.InstancePtr);
      var handle = SMProcess.Memory.Read<IntPtr>(new IntPtr(smMain + SM17Natives.TControl.HandleOffset));

      var restoreWndProcAddr = SM17Natives.TApplication.TApplicationOnMessagePtr.Read<int>(SMProcess.Memory);
      SM17Natives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                   WndProcHookAddr);

      WindowHelper.PostMessage(handle,
                               2345,
                               new IntPtr((int)InjectLibMessages.ExecuteOnMainThread),
                               new IntPtr(0));

      SMA.Instance.SMMgmt.MainThreadReady.WaitOne(AssemblyFactory.ExecutionTimeout);

      SM17Natives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                   restoreWndProcAddr);

      ExecCtxt.ExecutionParameters = null;

      return ExecCtxt.ExecutionResult;
    }

    //
    // SM-App Lifecycle

    protected virtual void OnSMExited(object    called,
                                      EventArgs args)
    {
      Dispose();
    }

    protected virtual void OnPreInit()
    {
      SMA.Instance.OnSMStartingImpl(this);
    }

    protected virtual void OnPostInit()
    {
      SMA.Instance.OnSMStartedImpl();

      IgnoreUserConfirmationPtr = SMProcess[SM17Natives.Globals.IgnoreUserConfirmationPtr];
    }

    #endregion




    #region Methods Abs

    protected abstract IEnumerable<ISMHookIO> GetIOCallbacks();


    //
    // ISuperMemo Methods

    public abstract SMAppVersion AppVersion { get; }

    #endregion




    #region Events

    public event EventHandler<SMProcessArgs> OnSMStoppedEvent;

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
