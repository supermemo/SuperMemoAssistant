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
// Created On:   2018/06/01 13:51
// Modified On:  2018/12/23 07:02
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;

namespace SuperMemoAssistant.Hooks
{
  public abstract class SMHookCallback
    : MarshalByRefObject,
      ISMHookSystem, ISMHookIO
  {
    #region Methods Impl

    /// <inheritdoc />
    public override object InitializeLifetimeService()
    {
      return null;
    }

    #endregion




    #region Methods Abs

    public abstract bool OnHookInstalled(bool      success,
                                         Exception ex = null);

    public abstract void Debug(string          msg,
                               params object[] args);

    public abstract void KeepAlive();


    public abstract void OnException(Exception ex);

    public abstract void SetWndProcHookAddr(int addr);

    public abstract bool OnUserMessage(int wParam);

    public abstract void GetExecutionParameters(out int       method,
                                                out dynamic[] parameters);

    public abstract void SetExecutionResult(int result);
    
    public abstract Dictionary<string, int> GetPatternsHintAddresses();

    public abstract void SetPatternsHintAddresses(Dictionary<string, int> hintAddrs);

    public abstract IEnumerable<string> GetTargetFilePaths();


    public abstract void OnFileCreate(String filePath,
                                      IntPtr fileHandle);

    public abstract void OnFileSeek(IntPtr fileHandle,
                                    UInt32 position);

    public abstract void OnFileWrite(IntPtr fileHandle,
                                     Byte[] buffer,
                                     UInt32 count);

    public abstract void OnFileClose(IntPtr fileHandle);

    #endregion
  }
}
