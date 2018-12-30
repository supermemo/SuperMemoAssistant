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
// Modified On:  2018/12/30 03:08
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Process.NET;
using Process.NET.Marshaling;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using Process.NET.Threads;
using SuperMemoAssistant.Hooks.SuperMemo;
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

    protected IPointer TApplicationInstancePtr { get; }

    protected ManagedWndProc WndProcDelegate { get; }

    protected Dictionary<NativeMethod, int> CallTable { get; } = new Dictionary<NativeMethod, int>();

    #endregion




    #region Constructors

    public unsafe SMHooks()
    {
      SMProcess = new ProcessSharp(System.Diagnostics.Process.GetCurrentProcess(),
                                   MemoryType.Local);
      TApplicationInstancePtr = SMProcess[SMNatives.TApplication.TApplicationInstanceAddr];

      // WndProc
      WndProcDelegate = new ManagedWndProc(WndProc);
      WndProcWrapper.SetCallback(Marshal.GetFunctionPointerForDelegate(WndProcDelegate));

      // Native calls
      SetupNativeMethods();
    }

    /// <inheritdoc />
    public void Dispose()
    {
      SMNatives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                 0);
    }

    #endregion




    #region Methods

    //
    // WndProc

    public bool InstallWndProcHook()
    {
      bool                       resumeThreads = false;
      IEnumerable<IRemoteThread> threads       = null;

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

        SMNatives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
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

    protected unsafe void WndProc(int   _,
                                  TMsg* msgPtr,
                                  bool* handled)
    {
      if (msgPtr->msg == (int)WindowsMessages.Quit)
      {
        Dispose();

        return;
      }

      if (msgPtr->msg != (int)WindowsMessages.User)
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
      catch (Exception ex)
      {
        throw;
      }
    }

    //
    // Native calls

    protected void SetupNativeMethods()
    {
      var scanner = new PatternScanner(SMProcess.ModuleFactory.MainModule);

      CallTable[NativeMethod.TSMMainSelectDefaultConcept] =
        scanner.Find(SMNatives.TSMMain.SelectDefaultConceptCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.TRegistryAddMember]        = scanner.Find(SMNatives.TRegistry.AddMember).BaseAddress.ToInt32();
      CallTable[NativeMethod.TRegistryImportFile]       = scanner.Find(SMNatives.TRegistry.ImportFile).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwGoToElement]          = scanner.Find(SMNatives.TElWind.GoToElementCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwPasteElement]         = scanner.Find(SMNatives.TElWind.PasteElementCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwAppendElement]        = scanner.Find(SMNatives.TElWind.AppendElementCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwAddElementFromText]   = scanner.Find(SMNatives.TElWind.AddElementFromTextCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwDeleteCurrentElement] = scanner.Find(SMNatives.TElWind.DeleteCurrentElementCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwGetText]              = scanner.Find(SMNatives.TElWind.GetTextCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwEnterUpdateLock]      = scanner.Find(SMNatives.TElWind.EnterUpdateLockCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwQuitUpdateLock]       = scanner.Find(SMNatives.TElWind.QuitUpdateLockCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwDone]                 = scanner.Find(SMNatives.TElWind.DoneSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwPasteArticle]         = scanner.Find(SMNatives.TElWind.PasteArticleSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.ElWdwSetText]              = scanner.Find(SMNatives.TElWind.SetTextCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.TCompDataGetType] =
        scanner.Find(SMNatives.TElWind.TComponentData.GetTypeCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.TCompDataGetText] =
        scanner.Find(SMNatives.TElWind.TComponentData.GetTextCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.TCompDataSetText] =
        scanner.Find(SMNatives.TElWind.TComponentData.SetTextCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.TCompDataGetTextRegMember] =
        scanner.Find(SMNatives.TElWind.TComponentData.GetTextRegMemberCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.TCompDataSetTextRegMember] =
        scanner.Find(SMNatives.TElWind.TComponentData.SetTextRegMemberCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.TCompDataGetImageRegMember] =
        scanner.Find(SMNatives.TElWind.TComponentData.GetImageRegMemberCallSig).BaseAddress.ToInt32();
      CallTable[NativeMethod.TCompDataSetImageRegMember] =
        scanner.Find(SMNatives.TElWind.TComponentData.SetImageRegMemberCallSig).BaseAddress.ToInt32();
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

            int elemId = registerCall3(CallTable[NativeMethod.ElWdwAppendElement],
                                       elWdw,
                                       elType,
                                       0);

            if (elemId <= 0)
              return -1;

            int res = registerCall2(CallTable[NativeMethod.ElWdwAddElementFromText],
                                    elWdw,
                                    elDesc);

            return res > 0 ? elemId : -1;
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
