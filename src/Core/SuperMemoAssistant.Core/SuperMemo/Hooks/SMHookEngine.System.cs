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




namespace SuperMemoAssistant.SuperMemo.Hooks
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.IO;
  using System.Windows;
  using Anotar.Serilog;
  using Extensions;
  using SMA;

  public partial class SMHookEngine
  {
    #region Methods Impl

    public void KeepAlive() { }

    /// <inheritdoc />
    public void OnException(Exception ex)
    {
      switch (ex)
      {
        case FileNotFoundException nativeLibEx
          when nativeLibEx.FileName.Contains("SuperMemoAssistant.Hooks.NativeLib"):
          LogTo.Warning(nativeLibEx, @"SuperMemoAssistant.Hooks.NativeLib.dll failed to load.
This might mean SuperMemoAssistant.Hooks.NativeLib.dll is missing from your SMA install location.
But most likely, NativeLib exists and failed to load other assemblies or libraries. If this is your case, try installing vcredist 2012 x86");
          break;

        default:
          LogTo.Warning(ex, "Exception caught in InjectLib.\r\n {Ex}", ex);
          break;
      }

      StopIPCServer();
      Application.Current.Shutdown(1);
    }

    [SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "<Pending>")]
    public void Debug(string          msg,
                      params object[] args)
    {
      LogTo.Debug(msg, args);
    }

    /// <inheritdoc />
    public Dictionary<string, int> GetPatternsHintAddresses()
    {
      return Core.CoreConfig.SuperMemo.PatternsHintAddresses;
    }

    /// <inheritdoc />
    public void SetPatternsHintAddresses(Dictionary<string, int> hintAddrs)
    {
      Core.CoreConfig.SuperMemo.PatternsHintAddresses = hintAddrs;
      Core.SMA.SaveConfigAsync().RunAsync();
    }

    #endregion
  }
}
