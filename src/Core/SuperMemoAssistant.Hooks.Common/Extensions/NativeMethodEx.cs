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
// Created On:   2020/01/11 13:56
// Modified On:  2020/01/11 14:00
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SuperMemoAssistant.SuperMemo.Common;

namespace SuperMemoAssistant.Extensions
{
  public static class NativeMethodEx
  {
    #region Methods

    public static Task<int> GetTask(this NativeMethod remoteTask)
    {
      AsyncManualResetEvent taskCompletedEvent = new AsyncManualResetEvent();
      int                   ret                = -1;

      void Signal(int       result,
                  Exception ex)
      {
        if (ex != null)
          throw ex;

        ret = result;
        taskCompletedEvent.Set();
      }

      async Task<int> WaitForSignal()
      {
        await taskCompletedEvent.WaitAsync();

        return ret;
      }

      //remoteTask.SetCallback(new ActionProxy<T, Exception>(Signal));

      return Task.Run(WaitForSignal);
    }

    public static TaskAwaiter<int> GetAwaiter(this NativeMethod remoteTask)
    {
      return remoteTask.GetTask().GetAwaiter();
    }

    #endregion
  }
}
