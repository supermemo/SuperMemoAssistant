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
// Created On:   2018/12/21 17:12
// Modified On:  2019/01/14 18:38
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Process.NET;
using Process.NET.Marshaling;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using SuperMemoAssistant.SuperMemo.SuperMemo17;
using SuperMemoAssistantHooksNativeLib;

// ReSharper disable RedundantDelegateCreation
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Hooks.InjectLib
{
  public class SMHooks : IDisposable
  {
    #region Properties & Fields - Non-Public

    protected ProcessSharp SMProcess { get; }

    protected ManagedWndProc WndProcDelegate { get; }

    protected Dictionary<NativeMethod, int> CallTable { get; } = new Dictionary<NativeMethod, int>();

    #endregion




    #region Constructors

    public unsafe SMHooks()
    {
      SMProcess = new ProcessSharp(System.Diagnostics.Process.GetCurrentProcess(),
                                   MemoryType.Local);

      // WndProc
      WndProcDelegate = new ManagedWndProc(WndProc);
      WndProcWrapper.SetCallback(Marshal.GetFunctionPointerForDelegate(WndProcDelegate));

      // Native calls
      SetupNativeMethods();
    }

    /// <inheritdoc />
    public void Dispose()
    {
      SM17Natives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                   0);
    }

    #endregion




    #region Methods

    //
    // WndProc

    public int GetWndProcNativeWrapperAddr()
    {
      return WndProcWrapper.GetWndProcNativeWrapperAddr();
    }

#if false
    public bool InstallWndProcHook()
    {
      bool                       resumeThreads = false;
      IEnumerable<IRemoteThread> threads = null;

      try
      {
        while (TApplicationInstancePtr.Read<int>() == 0)
          Thread.Sleep(100);

#pragma warning disable CS0618 // Type or member is obsolete
        var currentThreadId = AppDomain.GetCurrentThreadId();
#pragma warning restore CS0618 // Type or member is obsolete
        threads = SMProcess.ThreadFactory.RemoteThreads.Where(t => t.Id != currentThreadId);

        foreach (var remoteThread in threads)
          remoteThread.Suspend();

        resumeThreads = true;

        var wrapperAddr = WndProcWrapper.GetWndProcNativeWrapperAddr();

        SM17Natives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                   wrapperAddr);

        return true;
      }
      catch (Exception ex)
      {
        SMInject.Instance.OnException(ex);
        return false;
      }
      finally
      {
        if (resumeThreads)
          foreach (var remoteThread in threads)
            remoteThread.Resume();
      }
    }
#endif

    protected unsafe void WndProc(int   _,
                                  TMsg* msgPtr,
                                  bool* handled)
    {
      if (msgPtr->msg == (int)WindowsMessages.Quit)
      {
        Dispose();

        return;
      }

      if (msgPtr->msg != 2345)
        return;

      try
      {
        int wParam = msgPtr->wParam;

        switch (wParam)
        {
          case 9100101:
            int res = int.MinValue;
            SMInject.Instance.Debug("ExecuteOnMainThread");
            try
            {
              SMInject.Instance.Callback.GetExecutionParameters(out var method,
                                                                out var parameters);

              SMInject.Instance.Debug($"Executing {method} with {parameters}");
              res = CallNativeMethod((NativeMethod)method,
                                     parameters);
              SMInject.Instance.Debug($"Exec result = {res}");
            }
            catch (Exception ex)
            {
              SMInject.Instance.OnException(ex);
            }
            finally
            {
              SMInject.Instance.Callback.SetExecutionResult(res);
            }

            *handled = true;
            break;

          default:
            *handled = SMInject.Instance.OnUserMessage(msgPtr->wParam);
            break;
        }
      }
      catch (Exception)
      {
        // Ignored
      }
    }

    //
    // Native calls

    protected void SetupNativeMethods()
    {
      var scanner   = new PatternScanner(SMProcess.ModuleFactory.MainModule);
      var hintAddrs = SMInject.Instance.Callback.GetPatternsHintAddresses();

      foreach (var methodPattern in SM17Natives.MethodsPatterns)
      {
        int hintAddr = 0;

        if (hintAddrs.ContainsKey(methodPattern.Value.PatternText))
          hintAddr = hintAddrs[methodPattern.Value.PatternText];

        var scanRes = scanner.Find(methodPattern.Value,
                                   hintAddr);
        var procAddr = scanRes.BaseAddress.ToInt32();

        hintAddrs[methodPattern.Value.PatternText] = scanRes.Offset;
        CallTable[methodPattern.Key]               = procAddr;
      }

      SMInject.Instance.Callback.SetPatternsHintAddresses(hintAddrs);
    }

    protected int CallNativeMethod(NativeMethod method,
                                   dynamic[]    parameters)
    {
      var marshalledParameters =
        parameters.Select(p => MarshalValue.Marshal(SMProcess,
                                                    p))
                  .Cast<IMarshalledValue>().ToArray();

      try
      {
        switch (method)
        {
          case NativeMethod.AppendAndAddElementFromText:
            var elWdw  = marshalledParameters[0].Reference.ToInt32();
            var elType = marshalledParameters[1].Reference.ToInt32();
            var elDesc = marshalledParameters[2].Reference.ToInt32();

            // elWdw.AppendElement(elType, automatic: false); 
            int elemId = registerCall3(CallTable[NativeMethod.ElWdwAppendElement],
                                       elWdw,
                                       elType,
                                       0);

            if (elemId <= 0)
              return -1;

            // elWdw.AddElementFromText(elDesc);
            int res = registerCall2(CallTable[NativeMethod.ElWdwAddElementFromText],
                                    elWdw,
                                    elDesc);

            return res > 0 ? elemId : -1;


          case NativeMethod.PostponeRepetition:
            elWdw = marshalledParameters[0].Reference.ToInt32();
            var interval = marshalledParameters[1].Reference.ToInt32();

            // elWdw.ExecuteUncommitedRepetition(inclTopics: true, forceDisplay: false);
            registerCall3(CallTable[NativeMethod.ElWdwExecuteUncommitedRepetition],
                          elWdw,
                          1,
                          0);

            // elWdw.ScheduleInInterval(interval);
            registerCall2(CallTable[NativeMethod.ElWdwScheduleInInterval],
                          elWdw,
                          interval);

            // elWdw.SetElementState(DisplayState.Display);
            //registerCall2(CallTable[NativeMethod.ElWdwSetElementState],
            //              elWdw,
            //              2);

            // elWdw.NextElementInLearningQueue()
            registerCall1(CallTable[NativeMethod.ElWdwNextElementInLearningQueue],
                          elWdw);

            return 1;

          case NativeMethod.ForceRepetitionAndResume:
            elWdw    = marshalledParameters[0].Reference.ToInt32();
            interval = marshalledParameters[1].Reference.ToInt32();
            var adjustPriority = marshalledParameters[2].Reference.ToInt32();

            // elWdw.ForceRepetitionExt(interval, adjustPriority);
            registerCall3(CallTable[NativeMethod.ElWdwForceRepetitionExt],
                          elWdw,
                          interval,
                          adjustPriority);

            // elWdw.NextElementInLearningQueue();
            registerCall1(CallTable[NativeMethod.ElWdwNextElementInLearningQueue],
                          elWdw);

            // elWdw.RestoreLearningMode();
            registerCall1(CallTable[NativeMethod.ElWdwRestoreLearningMode],
                          elWdw);

            return 1;
        }

        switch (parameters.Length)
        {
          case 1:
            return registerCall1(CallTable[method],
                                 marshalledParameters[0].Reference.ToInt32());

          case 2:
            return registerCall2(CallTable[method],
                                 marshalledParameters[0].Reference.ToInt32(),
                                 marshalledParameters[1].Reference.ToInt32());

          case 3:
            return registerCall3(CallTable[method],
                                 marshalledParameters[0].Reference.ToInt32(),
                                 marshalledParameters[1].Reference.ToInt32(),
                                 marshalledParameters[2].Reference.ToInt32());

          case 4:
            return registerCall4(CallTable[method],
                                 marshalledParameters[0].Reference.ToInt32(),
                                 marshalledParameters[1].Reference.ToInt32(),
                                 marshalledParameters[2].Reference.ToInt32(),
                                 marshalledParameters[3].Reference.ToInt32());

          case 5:
            return registerCall5(CallTable[method],
                                 marshalledParameters[0].Reference.ToInt32(),
                                 marshalledParameters[1].Reference.ToInt32(),
                                 marshalledParameters[2].Reference.ToInt32(),
                                 marshalledParameters[3].Reference.ToInt32(),
                                 marshalledParameters[4].Reference.ToInt32());

          case 6:
            return registerCall6(CallTable[method],
                                 marshalledParameters[0].Reference.ToInt32(),
                                 marshalledParameters[1].Reference.ToInt32(),
                                 marshalledParameters[2].Reference.ToInt32(),
                                 marshalledParameters[3].Reference.ToInt32(),
                                 marshalledParameters[4].Reference.ToInt32(),
                                 marshalledParameters[5].Reference.ToInt32());

          default:
            throw new NotImplementedException($"No execution path to handle {parameters.Length} parameters.");
        }
      }
      finally
      {
        foreach (var param in marshalledParameters)
          param.Dispose();
      }
    }


    //
    // Delphi bridge

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    private static extern int registerCall1(
      int functionPtr,
      int arg1);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    private static extern int registerCall2(
      int functionPtr,
      int arg1,
      int arg2);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    private static extern int registerCall3(
      int functionPtr,
      int arg1,
      int arg2,
      int arg3);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    private static extern int registerCall4(
      int functionPtr,
      int arg1,
      int arg2,
      int arg3,
      int arg4);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    private static extern int registerCall5(
      int functionPtr,
      int arg1,
      int arg2,
      int arg3,
      int arg4,
      int arg5);

    [DllImport("SuperMemoAssistant.Hooks.NativeLib.dll",
      CallingConvention = CallingConvention.StdCall)]
    private static extern int registerCall6(
      int functionPtr,
      int arg1,
      int arg2,
      int arg3,
      int arg4,
      int arg5,
      int arg6);

    #endregion




    protected unsafe delegate void ManagedWndProc(int   smMain,
                                                  TMsg* msgAddr,
                                                  bool* handled);


    protected struct TMsg
    {
      public int hwnd;
      public int msg;
      public int wParam;
      public int lParam;
      public int time;
      public int pt;
    }
  }
}
