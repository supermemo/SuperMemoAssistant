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
// Created On:   2019/01/19 04:25
// Modified On:  2022/12/17 04:30
// Modified By:  - Alexis
//               - Ki

#endregion




using System;
using System.Collections.Generic;

namespace SuperMemoAssistant.SMA.Hooks
{
  public interface ISMAHookSystem
  {
    //
    // System
    void OnException(Exception ex);

    void SetWndProcHookAddr(int addr);

    bool OnUserMessage(int wParam);

    void GetExecutionParameters(out int       method,
                                out dynamic[] parameters);

    void SetExecutionResult(int result, dynamic outParameter);

    Dictionary<string, int> GetPatternsHintAddresses();
    void                    SetPatternsHintAddresses(Dictionary<string, int> hintAddrs);
  }
}
