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
// Created On:   2020/03/29 00:20
// Modified On:  2022/12/17 10:31
// Modified By:  - Alexis
//               - Ki

#endregion






// ReSharper disable RedundantDelegateCreation
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.Hooks.InjectLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Runtime.InteropServices;
  using System.Runtime.Remoting;
  using System.Windows.Forms;
  using Extensions;
  using Process.NET;
  using Process.NET.Marshaling;
  using Process.NET.Memory;
  using Process.NET.Native.Types;
  using Process.NET.Patterns;
  using Process.NET.Types;
  using SMA.Hooks;
  using SuperMemo;
  using SuperMemoAssistantHooksNativeLib;

  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class SMInject
  {
    #region Properties & Fields - Non-Public

    private readonly Dictionary<NativeMethod, int> _callTable = new Dictionary<NativeMethod, int>();

    private ProcessSharp   _smProcess;
    private ManagedWndProc _wndProcDelegate;

    #endregion




    #region Methods

    /// <summary>
    ///   Scan SuperMemo to find the methods matching the signatures provided by <paramref name="nativeData" />. Also sets up
    ///   the WndProc detour.
    /// </summary>
    /// <param name="nativeData">The offsets and method signatures for the running SuperMemo version</param>
    private unsafe void InstallSM(NativeData nativeData)
    {
      _smProcess = new ProcessSharp(System.Diagnostics.Process.GetCurrentProcess(),
                                    MemoryType.Local);

      // WndProc
      _wndProcDelegate = new ManagedWndProc(WndProc);

      WndProcWrapper.SetCallback(Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
      SMA.SetWndProcHookAddr(WndProcWrapper.GetWndProcNativeWrapperAddr());

      // Native calls
      ScanSMMethods(nativeData);
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

        var scanRes = scanner.Find(pattern,
                                   hintAddr);
        var procAddr = scanRes.BaseAddress.ToInt32();

        hintAddrs[pattern.PatternText] = scanRes.Offset;
        _callTable[method]             = procAddr;
      }

      SMA.SetPatternsHintAddresses(hintAddrs);
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
        try
        {
          SMA.Debug($"Received WndProc message {msgPtr->msg} with wParam {msgPtr->wParam}.");
        }
        catch (RemotingException) { }

        if (msgPtr       == null
          || msgPtr->msg == (int)WindowsMessages.Quit
          || msgPtr->msg != (int)InjectLibMessageId.SMA)
          return;

        int wParam = msgPtr->wParam;
        dynamic outParameter = null;

        switch ((InjectLibMessageParam)wParam)
        {
          case InjectLibMessageParam.ExecuteOnMainThread:
            int res = int.MinValue;

            try
            {
              SMA.GetExecutionParameters(
                out var method,
                out var parameters);

              res = CallNativeMethod((NativeMethod)method, parameters, out outParameter);
            }
            catch (Exception ex)
            {
              OnException(ex);
            }
            finally
            {
              SMA.SetExecutionResult(res, outParameter);
            }

            *handled = true;
            break;

          case InjectLibMessageParam.AttachDebugger:
            if (Debugger.IsAttached == false)
              Debugger.Launch();

            else
              Debugger.Break();
            break;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
    private int CallNativeMethod(NativeMethod method,
                                 dynamic[]    parameters,
                                 out dynamic  outParameter)
    {
      SMA.Debug($"Executing native method {Enum.GetName(typeof(NativeMethod), method)}.");

      outParameter = null;
      if (parameters == null)
      {
        OnException(new ArgumentNullException(nameof(parameters), $"CallNativeMethod: Called with null 'parameters' for method {method}"));
        return -1;
      }

      // Possible null reference on parameters
      var marshalledParameters = new IMarshalledValue[parameters.Length];

      for (var i = 0; i < parameters.Length; i++)
      {
        var p             = parameters[i];
        var dynMarshalled = MarshalValue.Marshal(_smProcess, p);

        if (dynMarshalled is IMarshalledValue marshalled)
        {
          marshalledParameters[i] = marshalled;
        }

        else
        {
          OnException(new ArgumentException($"CallNativeMethod: Parameter n°{i} '{p}' could not be marshalled for method {method}",
                                            nameof(parameters)));
          return -1;
        }
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
          case NativeMethod.ElWdw_GetElementAsText:
            elWdw   = marshalledParameters[0].Reference.ToInt32();
            var marshalled = MarshalValue.Marshal(_smProcess, new DelphiUTF8String(8000)); // I'm using this because I think it allocates memory
            var strAddress = marshalled.Reference.ToInt32();
            var ret = Delphi.registerCall2(_callTable[NativeMethod.ElWdw_GetElementAsText],
                                 elWdw,
                                 strAddress);

            unsafe
            {
              outParameter = Marshal.PtrToStringAnsi(new IntPtr(*(int*)(marshalled.Reference.ToPointer())));
            }
            marshalled.Dispose();
            return ret;
          case NativeMethod.Priority_SetPriority:
            return Delphi.registerCallDouble1(_callTable[method],
                                        marshalledParameters[0].Reference.ToInt32(),
                                        marshalledParameters[1].Reference.ToInt32());
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
        {
          param.Dispose();
        }
      }
    }

    #endregion




    internal unsafe delegate void ManagedWndProc(int          smMain,
                                                 Delphi.TMsg* msgAddr,
                                                 bool*        handled);
  }
}
