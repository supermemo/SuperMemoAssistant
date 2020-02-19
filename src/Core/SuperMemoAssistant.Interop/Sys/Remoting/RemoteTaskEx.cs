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
// Modified On:  2020/02/17 17:42
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using Nito.AsyncEx;

namespace SuperMemoAssistant.Sys.Remoting
{
  public static class RemoteTaskEx
  {
    #region Methods

    public static Task GetTask(this RemoteTask remoteTask)
    {
      AsyncManualResetEvent taskCompletedEvent = new AsyncManualResetEvent();

      void Signal(Exception ex)
      {
        if (ex != null)
        {
          LogTo.Warning(ex, "Exception caught");
          throw ex;
        }

        taskCompletedEvent.Set();
      }

      async Task WaitForSignal()
      {
        await taskCompletedEvent.WaitAsync();
      }

      remoteTask.SetCallback(new ActionProxy<Exception>(Signal));

      return Task.Run(WaitForSignal);
    }

    public static TaskAwaiter GetAwaiter(this RemoteTask remoteTask)
    {
      return remoteTask.GetTask().GetAwaiter();
    }

    public static RemoteTask ConfigureRemoteTask(this Task task, Action<Exception> onExceptionHandler)
    {
      return new RemoteTask(task, onExceptionHandler);
    }

    public static Task<T> GetTask<T>(this RemoteTask<T> remoteTask)
    {
      AsyncManualResetEvent taskCompletedEvent = new AsyncManualResetEvent();
      T                     ret                = default;

      void Signal(T         result,
                  Exception ex)
      {
        if (ex != null)
        {
          LogTo.Warning(ex, "Exception caught");
          throw ex;
        }

        ret = result;
        taskCompletedEvent.Set();
      }

      async Task<T> WaitForSignal()
      {
        var cts = new CancellationTokenSource(60000);
        await taskCompletedEvent.WaitAsync(cts.Token);

        return ret;
      }

      remoteTask.SetCallback(new ActionProxy<T, Exception>(Signal));

      return Task.Run(WaitForSignal);
    }

    public static TaskAwaiter<T> GetAwaiter<T>(this RemoteTask<T> remoteTask)
    {
      return remoteTask.GetTask().GetAwaiter();
    }

    public static T GetResult<T>(this RemoteTask<T> remoteTask)
    {
      return remoteTask.GetAwaiter().GetResult();
    }

    public static RemoteTask<T> ConfigureRemoteTask<T>(this Task<T> task, Action<Exception> onExceptionHandler)
    {
      return new RemoteTask<T>(task, onExceptionHandler);
    }

    #endregion
  }
}
