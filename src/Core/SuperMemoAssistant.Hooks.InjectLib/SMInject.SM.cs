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
// Modified On:  2019/02/25 00:00
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Process.NET;
using Process.NET.Marshaling;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using SuperMemoAssistant.SMA.Hooks;
using SuperMemoAssistant.SuperMemo.Common;
using SuperMemoAssistant.SuperMemo.SuperMemo17;
using SuperMemoAssistantHooksNativeLib;

// ReSharper disable RedundantDelegateCreation
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Hooks.InjectLib
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class SMInject
  {
    #region Properties & Fields - Non-Public

    private readonly Dictionary<NativeMethod, int> _callTable = new Dictionary<NativeMethod, int>();

    private ProcessSharp   _smProcess;
    private ManagedWndProc _wndProcDelegate;

    #endregion




    #region Methods

    private unsafe void InstallSM()
    {
      _smProcess = new ProcessSharp(System.Diagnostics.Process.GetCurrentProcess(),
                                    MemoryType.Local);

      // WndProc
      _wndProcDelegate = new ManagedWndProc(WndProc);

      WndProcWrapper.SetCallback(Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
      SMA.SetWndProcHookAddr(WndProcWrapper.GetWndProcNativeWrapperAddr());

      // Native calls
      ScanSMMethods();
    }

    protected void ScanSMMethods()
    {
      var scanner   = new PatternScanner(_smProcess.ModuleFactory.MainModule);
      var hintAddrs = SMA.GetPatternsHintAddresses();

      foreach (var methodPattern in SM17Natives.MethodsPatterns)
      {
        int hintAddr = 0;

        if (hintAddrs.ContainsKey(methodPattern.Value.PatternText))
          hintAddr = hintAddrs[methodPattern.Value.PatternText];

        var scanRes = scanner.Find(methodPattern.Value,
                                   hintAddr);
        var procAddr = scanRes.BaseAddress.ToInt32();

        hintAddrs[methodPattern.Value.PatternText] = scanRes.Offset;
        _callTable[methodPattern.Key]              = procAddr;
      }

      SMA.SetPatternsHintAddresses(hintAddrs);
    }


    private unsafe void WndProc(int          _,
                                Delphi.TMsg* msgPtr,
                                bool*        handled)
    {
      if (msgPtr->msg == (int)WindowsMessages.Quit
        || msgPtr->msg != (int)InjectLibMessageIds.SMA)
        return;

      try
      {
        int wParam = msgPtr->wParam;

        switch ((InjectLibMessageParams)wParam)
        {
          case InjectLibMessageParams.ExecuteOnMainThread:
            int res = int.MinValue;

            try
            {
              SMA.GetExecutionParameters(
                out var method,
                out var parameters);

              res = CallNativeMethod((NativeMethod)method,
                                     parameters);
            }
            catch (Exception ex)
            {
              OnException(ex);
            }
            finally
            {
              SMA.SetExecutionResult(res);
            }

            *handled = true;
            break;

          case InjectLibMessageParams.AttachDebugger:
            if (Debugger.IsAttached == false)
              Debugger.Launch();

            else
              Debugger.Break();
            break;
        }
      }
      catch (Exception)
      {
        // Ignored
      }
    }

    protected int CallNativeMethod(NativeMethod method,
                                   dynamic[]    parameters)
    {
      var marshalledParameters =
        parameters.Select(p => MarshalValue.Marshal(_smProcess, p))
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
            int elemId = Delphi.registerCall3(_callTable[NativeMethod.ElWdwAppendElement],
                                              elWdw,
                                              elType,
                                              0);

            if (elemId <= 0)
              return -1;

            // elWdw.AddElementFromText(elDesc);
            int res = Delphi.registerCall2(_callTable[NativeMethod.ElWdwAddElementFromText],
                                           elWdw,
                                           elDesc);

            return res > 0 ? elemId : -1;

          case NativeMethod.PostponeRepetition:
            elWdw = marshalledParameters[0].Reference.ToInt32();
            var interval = marshalledParameters[1].Reference.ToInt32();

            // elWdw.ExecuteUncommitedRepetition(inclTopics: true, forceDisplay: false);
            Delphi.registerCall3(_callTable[NativeMethod.ElWdwExecuteUncommitedRepetition],
                                 elWdw,
                                 1,
                                 0);

            // elWdw.ScheduleInInterval(interval);
            Delphi.registerCall2(_callTable[NativeMethod.ElWdwScheduleInInterval],
                                 elWdw,
                                 interval);

            // elWdw.SetElementState(DisplayState.Display);
            //registerCall2(_callTable[NativeMethod.ElWdwSetElementState],
            //              elWdw,
            //              2);

            // elWdw.NextElementInLearningQueue()
            Delphi.registerCall1(_callTable[NativeMethod.ElWdwNextElementInLearningQueue],
                                 elWdw);

            return 1;

          case NativeMethod.ForceRepetitionAndResume:
            elWdw    = marshalledParameters[0].Reference.ToInt32();
            interval = marshalledParameters[1].Reference.ToInt32();
            var adjustPriority = marshalledParameters[2].Reference.ToInt32();

            // elWdw.ForceRepetitionExt(interval, adjustPriority);
            Delphi.registerCall3(_callTable[NativeMethod.ElWdwForceRepetitionExt],
                                 elWdw,
                                 interval,
                                 adjustPriority);

            // elWdw.NextElementInLearningQueue();
            Delphi.registerCall1(_callTable[NativeMethod.ElWdwNextElementInLearningQueue],
                                 elWdw);

            // elWdw.RestoreLearningMode();
            Delphi.registerCall1(_callTable[NativeMethod.ElWdwRestoreLearningMode],
                                 elWdw);

            return 1;
        }

        switch (parameters.Length)
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
            throw new NotImplementedException($"No execution path to handle {parameters.Length} parameters.");
        }
      }
      finally
      {
        foreach (var param in marshalledParameters)
          param.Dispose();
      }
    }

    #endregion




    protected unsafe delegate void ManagedWndProc(int          smMain,
                                                  Delphi.TMsg* msgAddr,
                                                  bool*        handled);
  }
}
