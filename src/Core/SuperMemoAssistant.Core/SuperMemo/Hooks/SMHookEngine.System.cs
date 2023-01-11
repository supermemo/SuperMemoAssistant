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
// Modified On:  2022/12/17 15:11
// Modified By:  - Alexis
//               - Ki

#endregion




namespace SuperMemoAssistant.SuperMemo.Hooks
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading;
  using System.Windows;
  using Anotar.Serilog;
  using Extensions;
  using Process.NET.Assembly;
  using Process.NET.Utilities;
  using SMA;
  using SMA.Hooks;

  public partial class SMHookEngine
  {
    #region Properties & Fields - Non-Public

    private readonly ExecutionContext _execCtxt             = new ExecutionContext();
    private readonly ManualResetEvent _mainThreadReadyEvent = new ManualResetEvent(false);

    private int _wndProcHookAddr;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
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
          LogTo.Warning(ex, "Exception caught in InjectLib.\r\n {Ex}", ex);
          break;
      }

      StopIPCServer();

      Application.Current.Dispatcher.InvokeAsync(() =>
      {
        MessageBox.Show("A critical error occurred in the hook engine. SMA is shutting down.");
        Application.Current.Shutdown(1);
      });
    }

    /// <inheritdoc />
    public override Dictionary<string, int> GetPatternsHintAddresses()
    {
      return Core.CoreConfig.SuperMemo.PatternsHintAddresses;
    }

    /// <inheritdoc />
    public override void SetPatternsHintAddresses(Dictionary<string, int> hintAddrs)
    {
      Core.CoreConfig.SuperMemo.PatternsHintAddresses = hintAddrs;
      Core.SMA.SaveConfigAsync().RunAsync();
    }

    /// <inheritdoc />
    public override void SetWndProcHookAddr(int addr)
    {
      _wndProcHookAddr = addr;
    }

    /// <inheritdoc />
    public override bool OnUserMessage(int wParam)
    {
      switch ((InjectLibMessageParam)wParam)
      {
        case InjectLibMessageParam.ExecuteOnMainThread:
          //_mainThreadReadyEvent.Set();

          return true;
      }

      return false;
    }

    /// <inheritdoc />
    public override void GetExecutionParameters(out int       method,
                                                out dynamic[] parameters)
    {
      method     = (int)_execCtxt.ExecutionMethod;
      parameters = (dynamic[])_execCtxt.ExecutionParameters;
    }

    /// <inheritdoc />
    public override void SetExecutionResult(int result, dynamic outParameter)
    {
      _execCtxt.ExecutionResult = result;
      _execCtxt.ExecutionOutParameter = outParameter;
      _mainThreadReadyEvent.Set();
    }

    #endregion




    #region Methods

    // TODO: Not thread safe & ugly overall...
    public int ExecuteOnMainThread(NativeMethod method,
                                   dynamic[]    parameters,
                                   out dynamic  outParameter)
    {
      _mainThreadReadyEvent.Reset();

      _execCtxt.ExecutionResult     = 0;
      _execCtxt.ExecutionMethod     = method;
      _execCtxt.ExecutionParameters = parameters;
      _execCtxt.ExecutionOutParameter = null;

      var smMem = Core.SM.SMProcess.Memory;

      var smMain = smMem.Read<int>(Core.Natives.SMMain.InstancePtr);
      var handle = smMem.Read<IntPtr>(new IntPtr(smMain + Core.Natives.Control.HandleOffset));

      //var restoreWndProcAddr = Core.Natives.Application.TApplicationOnMessagePtr.Read<int>(smMem);
      Core.Natives.Application.TApplicationOnMessagePtr.Write<int>(smMem, _wndProcHookAddr);

      WindowHelper.PostMessage(handle,
                               (int)InjectLibMessageId.SMA,
                               new IntPtr((int)InjectLibMessageParam.ExecuteOnMainThread),
                               new IntPtr(0));

      _mainThreadReadyEvent.WaitOne(AssemblyFactory.ExecutionTimeout);

      // TODO: Caused exception issues
      //Core.Natives.Application.TApplicationOnMessagePtr.Write<int>(smMem, restoreWndProcAddr);
      Core.Natives.Application.TApplicationOnMessagePtr.Write<int>(smMem, 0);

      _execCtxt.ExecutionParameters = null;
      outParameter = _execCtxt.ExecutionOutParameter;

      return _execCtxt.ExecutionResult;
    }

    #endregion




    private class ExecutionContext
    {
      #region Properties & Fields - Public

      public NativeMethod         ExecutionMethod     { get; set; }
      public IEnumerable<dynamic> ExecutionParameters { get; set; }
      public dynamic              ExecutionOutParameter { get; set; }
      public int                  ExecutionResult     { get; set; }

      #endregion
    }
  }
}
