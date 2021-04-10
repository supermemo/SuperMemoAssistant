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




// ReSharper disable RedundantDelegateCreation
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Hooks.InjectLib
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Diagnostics.CodeAnalysis;
  using System.Runtime.InteropServices;
  using System.Threading;
  using Extensions;
  using Process.NET;
  using Process.NET.Marshaling;
  using Process.NET.Memory;
  using Process.NET.Patterns;
  using Process.NET.Utilities;
  using SMA.Hooks.Models;
  using SuperMemo;
  using SuperMemoAssistantHooksNativeLib;

  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class SMInject
  {
    #region Properties & Fields - Non-Public

    private readonly Dictionary<NativeMethod, int> _callTable = new Dictionary<NativeMethod, int>();

    private readonly AutoResetEvent _execAvailableEvent = new AutoResetEvent(false);
    private readonly object         _execHeldLock       = new Object();

    private readonly ConcurrentQueue<NativeExecutionParameters> _execQueue = new ConcurrentQueue<NativeExecutionParameters>();

    private bool _execHeld;
    private bool _execRequested;

    private IPointer _smAppMessagePtr;
    private IPointer _smMainHandlePtr;

    private ProcessSharp _smProcess;

    /// <summary>Prevents the delegate from being collected</summary>
    private ManagedWndProc _wndProcDelegate;

    private int _wndProcHookAddr;

    #endregion




    #region Methods Impl

    public void RequestExecution(IEnumerable<NativeExecutionParameters> execParams)
    {
      lock (_execHeldLock)
        foreach (var ep in execParams)
          RequestExecutionInternal(ep);
    }

    public void RequestExecution(NativeExecutionParameters execParams)
    {
      lock (_execHeldLock)
        RequestExecutionInternal(execParams);
    }

    #endregion




    #region Methods

    /// <summary>
    ///   Scan SuperMemo to find the methods matching the signatures provided by <paramref name="nativeData" />. Also sets up
    ///   the WndProc detour.
    /// </summary>
    /// <param name="nativeData">The offsets and method signatures for the running SuperMemo version</param>
    private void InstallSM(NativeData nativeData)
    {
      _smProcess = new ProcessSharp(Process.GetCurrentProcess(),
                                    MemoryType.Local);

      SetupWndProc(nativeData);
      ScanSMMethods(nativeData);
    }

    private unsafe void SetupWndProc(NativeData nativeData)
    {
      var smMainInstancePtr = new IntPtr(nativeData.Pointers[NativePointer.SMMain_InstancePtr]);
      var handleOffset      = nativeData.Pointers[NativePointer.Control_HandleOffset];

      var applicationInstancePtr = new IntPtr(nativeData.Pointers[NativePointer.Application_InstancePtr]);
      var onMessageOffset        = nativeData.Pointers[NativePointer.Application_OnMessageOffset];

      _smMainHandlePtr = _smProcess[new ObjPtr(smMainInstancePtr, handleOffset)];
      _smAppMessagePtr = _smProcess[new ObjPtr(applicationInstancePtr, onMessageOffset)];

      _wndProcDelegate = new ManagedWndProc(WndProc);

      WndProcWrapper.SetCallback(Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
      _wndProcHookAddr = WndProcWrapper.GetWndProcNativeWrapperAddr();
    }

    private void ScanSMMethods(NativeData nativeData)
    {
      var scanner   = new PatternScanner(_smProcess.ModuleFactory.MainModule);
      var hintAddrs = SMA.GetPatternsHintAddresses();

      foreach (var (method, pattern) in nativeData.GetAllMemoryPatterns())
      {
        int hintAddr = 0;

        if (hintAddrs.ContainsKey(pattern.PatternText))
          hintAddr = hintAddrs[pattern.PatternText];

        var scanRes  = scanner.Find(pattern, hintAddr);
        var procAddr = scanRes.BaseAddress.ToInt32();

        hintAddrs[pattern.PatternText] = scanRes.Offset;
        _callTable[method]             = procAddr;
      }

      SMA.SetPatternsHintAddresses(hintAddrs);
    }

    public void RequestExecutionInternal(NativeExecutionParameters execParams)
    {
      if (_execHeld == false && _execRequested == false)
      {
        _smAppMessagePtr.Write<int>(0, _wndProcHookAddr);

        WindowHelper.PostMessage(_smMainHandlePtr.Read<IntPtr>(),
                                 (int)InjectLibMessageId.SMA,
                                 new IntPtr(0),
                                 new IntPtr(0));
      }

      _execQueue.Enqueue(execParams);
      _execAvailableEvent.Set();
    }

    private int ExecuteNextInQueue(
      NativeExecutionParameters execParams,
      ref bool                  shouldHoldMainThread)
    {
      int res = int.MinValue;

      try
      {
        switch (execParams.Type)
        {
          case InjectLibExecutionType.HoldMainThread:
            shouldHoldMainThread = true;
            break;

          case InjectLibExecutionType.ReleaseMainThread:
            shouldHoldMainThread = false;
            break;

          case InjectLibExecutionType.ExecuteOnMainThread:
            res                  = CallNativeMethod(execParams.Method, execParams.Parameters);
            shouldHoldMainThread = shouldHoldMainThread || execParams.ShouldHoldMainThread;

            return res;

          case InjectLibExecutionType.AttachDebugger:
            if (Debugger.IsAttached == false)
              Debugger.Launch();

            else
              Debugger.Break();
            break;

          default:
            OnException(new ArgumentException($"Invalid NativeExecutionParameters.Type {execParams.Type}."));
            break;
        }

        res = 1;
      }
      catch (Exception ex)
      {
        OnException(ex);
      }

      return res;
    }

    /// <summary>
    ///   This is a detour from SuperMemo's WndProc method. This is used to hijack the main thread and execute code in
    ///   SuperMemo in a thread-safe way
    /// </summary>
    /// <param name="_"></param>
    /// <param name="msgPtr">The WndProc message</param>
    /// <param name="handled">Whether the message has been handled</param>
    private unsafe void WndProc(int          _,
                                Delphi.TMsg* msgPtr,
                                bool*        handled)
    {
      try
      {
        //try
        //{
        //  SMA.Debug($"Received WndProc message {msgPtr->msg} with wParam {msgPtr->wParam}.");
        //}
        //catch (RemotingException) { }

        if (msgPtr       == null
          || msgPtr->msg != (int)InjectLibMessageId.SMA)
          return;

        *handled = true;

        lock (_execHeldLock)
        {
          _smAppMessagePtr.Write<int>(0, 0);

          _execHeld      = true;
          _execRequested = false;
        }

        var shouldHoldMainThread = false;
        var idResultMap          = new Dictionary<int, int>();

        while (_execHeld)
        {
          while (_execQueue.TryDequeue(out var execParams))
            idResultMap[execParams.Id] = ExecuteNextInQueue(execParams, ref shouldHoldMainThread);

          SMA.SetExecutionResults(idResultMap);
          idResultMap.Clear();

          if (shouldHoldMainThread && _execAvailableEvent.WaitOne(3000))
            continue;

          lock (_execHeldLock)
          {
            if (_execQueue.IsEmpty == false)
              continue;

            _execHeld = false;
          }
        }
      }
      catch (Exception ex)
      {
        OnException(ex);
      }
    }


    /// <summary>Calls method <paramref name="method" /> in SuperMemo</summary>
    /// <param name="method">The method to call</param>
    /// <param name="parameters">The method's parameters to pass along with the call</param>
    /// <returns>The returned value (eax register) from the call to <paramref name="method" /></returns>
    [SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
    private int CallNativeMethod(NativeMethod        method,
                                 IEnumerable<object> parameters)
    {
      SMA.Debug($"Executing native method {Enum.GetName(typeof(NativeMethod), method)}.");

      if (parameters == null)
      {
        OnException(new ArgumentNullException(nameof(parameters), $"CallNativeMethod: Called with null 'parameters' for method {method}"));
        return -1;
      }

      // Possible null reference on parameters
      var marshalledParameters = new List<IMarshalledValue>();
      var paramIt              = 0;

      foreach (var param in parameters)
      {
        var dynMarshalled = MarshalValue.Marshal(_smProcess, (dynamic)param);

        if (dynMarshalled is IMarshalledValue marshalled)
        {
          marshalledParameters.Add(marshalled);
        }

        else
        {
          OnException(new ArgumentException(
                        $"CallNativeMethod: Parameter n°{paramIt} '{param}' could not be marshalled for method {method}",
                        nameof(parameters)));
          return -1;
        }

        paramIt++;
      }

      try
      {
        switch (method)
        {
          case NativeMethod.AppendAndAddElementFromText:
            var elWdw  = marshalledParameters[0].Reference.ToInt32();
            var elType = marshalledParameters[1].Reference.ToInt32();
            var elDesc = marshalledParameters[2].Reference.ToInt32();

            // elWdw.AppendElement(elType, automatic: false); 
            int elemId = Delphi.registerCall3(_callTable[NativeMethod.ElWdw_AppendElement],
                                              elWdw,
                                              elType,
                                              0);

            if (elemId <= 0)
              return -1;

            // elWdw.AddElementFromText(elDesc);
            int res = Delphi.registerCall2(_callTable[NativeMethod.ElWdw_AddElementFromText],
                                           elWdw,
                                           elDesc);

            return res > 0 ? elemId : -1;

          case NativeMethod.PostponeRepetition:
            elWdw = marshalledParameters[0].Reference.ToInt32();
            var interval = marshalledParameters[1].Reference.ToInt32();

            // elWdw.ExecuteUncommittedRepetition(inclTopics: true, forceDisplay: false);
            Delphi.registerCall3(_callTable[NativeMethod.ElWdw_ExecuteUncommittedRepetition],
                                 elWdw,
                                 1,
                                 0);

            // elWdw.ScheduleInInterval(interval);
            Delphi.registerCall2(_callTable[NativeMethod.ElWdw_ScheduleInInterval],
                                 elWdw,
                                 interval);

            // elWdw.SetElementState(DisplayState.Display);
            //registerCall2(_callTable[NativeMethod.ElWdw_SetElementState],
            //              elWdw,
            //              2);

            // elWdw.NextElementInLearningQueue()
            Delphi.registerCall1(_callTable[NativeMethod.ElWdw_NextElementInLearningQueue],
                                 elWdw);

            return 1;

          case NativeMethod.ForceRepetitionAndResume:
            elWdw    = marshalledParameters[0].Reference.ToInt32();
            interval = marshalledParameters[1].Reference.ToInt32();
            var adjustPriority = marshalledParameters[2].Reference.ToInt32();

            // elWdw.ForceRepetitionExt(interval, adjustPriority);
            Delphi.registerCall3(_callTable[NativeMethod.ElWdw_ForceRepetitionExt],
                                 elWdw,
                                 interval,
                                 adjustPriority);

            // elWdw.NextElementInLearningQueue();
            Delphi.registerCall1(_callTable[NativeMethod.ElWdw_NextElementInLearningQueue],
                                 elWdw);

            // elWdw.RestoreLearningMode();
            Delphi.registerCall1(_callTable[NativeMethod.ElWdw_RestoreLearningMode],
                                 elWdw);

            return 1;
        }

        switch (marshalledParameters.Count)
        {
          case 1:
            return Delphi.registerCall1(_callTable[method],
                                        marshalledParameters[0].Reference.ToInt32());

          case 2:
            return Delphi.registerCall2(_callTable[method],
                                        marshalledParameters[0].Reference.ToInt32(),
                                        marshalledParameters[1].Reference.ToInt32());

          case 3:
            return Delphi.registerCall3(_callTable[method],
                                        marshalledParameters[0].Reference.ToInt32(),
                                        marshalledParameters[1].Reference.ToInt32(),
                                        marshalledParameters[2].Reference.ToInt32());

          case 4:
            return Delphi.registerCall4(_callTable[method],
                                        marshalledParameters[0].Reference.ToInt32(),
                                        marshalledParameters[1].Reference.ToInt32(),
                                        marshalledParameters[2].Reference.ToInt32(),
                                        marshalledParameters[3].Reference.ToInt32());

          case 5:
            return Delphi.registerCall5(_callTable[method],
                                        marshalledParameters[0].Reference.ToInt32(),
                                        marshalledParameters[1].Reference.ToInt32(),
                                        marshalledParameters[2].Reference.ToInt32(),
                                        marshalledParameters[3].Reference.ToInt32(),
                                        marshalledParameters[4].Reference.ToInt32());

          case 6:
            return Delphi.registerCall6(_callTable[method],
                                        marshalledParameters[0].Reference.ToInt32(),
                                        marshalledParameters[1].Reference.ToInt32(),
                                        marshalledParameters[2].Reference.ToInt32(),
                                        marshalledParameters[3].Reference.ToInt32(),
                                        marshalledParameters[4].Reference.ToInt32(),
                                        marshalledParameters[5].Reference.ToInt32());

          default:
            throw new NotImplementedException($"No execution path to handle {marshalledParameters.Count} parameters.");
        }
      }
      finally
      {
        foreach (var param in marshalledParameters)
          param.Dispose();
      }
    }

    #endregion




    internal unsafe delegate void ManagedWndProc(int          smMain,
                                                 Delphi.TMsg* msgAddr,
                                                 bool*        handled);
  }
}
