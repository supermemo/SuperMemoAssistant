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
// Created On:   2019/08/09 11:38
// Modified On:  2020/01/12 14:59
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using Anotar.Serilog;
using Process.NET.Assembly;
using Process.NET.Utilities;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SMA.Hooks;

namespace SuperMemoAssistant.SuperMemo.Hooks
{
  public partial class SMHookEngine
  {
    #region Properties & Fields - Non-Public

    private readonly ExecutionContext _execCtxt             = new ExecutionContext();
    private readonly ManualResetEvent _mainThreadReadyEvent = new ManualResetEvent(false);

    private int _wndProcHookAddr;

    #endregion




    #region Methods Impl

    public override void OnException(Exception ex)
    {
      switch (ex)
      {
        case FileNotFoundException nativeLibEx
          when nativeLibEx.Message.StartsWith("Could not load file or assembly 'SuperMemoAssistant.Hooks.NativeLib",
                                              StringComparison.OrdinalIgnoreCase):
          LogTo.Warning(nativeLibEx, @"SuperMemoAssistant.Hooks.NativeLib.dll failed to load.
This might mean SuperMemoAssistant.Hooks.NativeLib.dll is missing from your SMA install location.
But most likely, NativeLib exists and failed to load other assemblies or libraries. If this is your case, try installing vcredist 2012 x86");
          break;

        default:
          LogTo.Warning(ex, $"Exception caught in InjectLib.\r\n {ex}");
          break;
      }

      StopIPCServer();
      Application.Current.Shutdown(1);
    }

    public override Dictionary<string, int> GetPatternsHintAddresses()
    {
      return Core.SMA.CoreConfig.SuperMemo.PatternsHintAddresses;
    }

    public override void SetPatternsHintAddresses(Dictionary<string, int> hintAddrs)
    {
      Core.SMA.CoreConfig.SuperMemo.PatternsHintAddresses = hintAddrs;
      Core.SMA.SaveConfig(false);
    }

    public override void SetWndProcHookAddr(int addr)
    {
      _wndProcHookAddr = addr;
    }

    /// <param name="wParam"></param>
    public override bool OnUserMessage(int wParam)
    {
      switch ((InjectLibMessageParams)wParam)
      {
        case InjectLibMessageParams.ExecuteOnMainThread:
          //_mainThreadReadyEvent.Set();

          return true;
      }

      return false;
    }

    public override void GetExecutionParameters(out int       method,
                                                out dynamic[] parameters)
    {
      method     = (int)_execCtxt.ExecutionMethod;
      parameters = _execCtxt.ExecutionParameters;
    }

    public override void SetExecutionResult(int result)
    {
      _execCtxt.ExecutionResult = result;
      _mainThreadReadyEvent.Set();
    }

    #endregion




    #region Methods

    // TODO: Not thread safe & ugly overall...
    public int ExecuteOnMainThread(NativeMethod method,
                                   dynamic[]    parameters)
    {
      _mainThreadReadyEvent.Reset();

      _execCtxt.ExecutionResult     = 0;
      _execCtxt.ExecutionMethod     = method;
      _execCtxt.ExecutionParameters = parameters;

      var smMem = Core.SM.SMProcess.Memory;

      var smMain = smMem.Read<int>(Core.Natives.SMMain.InstancePtr);
      var handle = smMem.Read<IntPtr>(new IntPtr(smMain + Core.Natives.Control.HandleOffset));

      //var restoreWndProcAddr = Core.Natives.Application.TApplicationOnMessagePtr.Read<int>(smMem);
      Core.Natives.Application.TApplicationOnMessagePtr.Write<int>(smMem, _wndProcHookAddr);

      WindowHelper.PostMessage(handle,
                               (int)InjectLibMessageIds.SMA,
                               new IntPtr((int)InjectLibMessageParams.ExecuteOnMainThread),
                               new IntPtr(0));

      _mainThreadReadyEvent.WaitOne(AssemblyFactory.ExecutionTimeout);

      // TODO: Caused exception issues
      //Core.Natives.Application.TApplicationOnMessagePtr.Write<int>(smMem, restoreWndProcAddr);
      Core.Natives.Application.TApplicationOnMessagePtr.Write<int>(smMem, 0);

      _execCtxt.ExecutionParameters = null;

      return _execCtxt.ExecutionResult;
    }

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
