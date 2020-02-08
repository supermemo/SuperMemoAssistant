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
// Created On:   2019/02/24 23:29
// Modified On:  2019/02/25 00:14
// Modified By:  Alexis

#endregion




using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading;
using Sentry;

namespace SuperMemoAssistant.Hooks.InjectLib
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class SMInject
  {
    #region Properties & Fields - Non-Public

    private readonly AutoResetEvent _dataAvailableEvent = new AutoResetEvent(false);
    private readonly ConcurrentQueue<(HookedFunction func, object[] datas)> _dataQueue =
      new ConcurrentQueue<(HookedFunction, object[])>();

    #endregion




    #region Methods

    private void Enqueue(HookedFunction  func,
                         params object[] data)
    {
      _dataQueue.Enqueue((func, data));
      _dataAvailableEvent.Set();
    }

    private void DispatchMessages()
    {
      while (HasExited == false)
      {
        _dataAvailableEvent.WaitOne(1000);

        while (_dataQueue.TryDequeue(out var data))
          ProcessData(data.func, data.datas);
      }
    }

    private void ProcessData(HookedFunction func,
                             object[]       data)
    {
      try
      {
        switch (func)
        {
          case HookedFunction.CreateFile:
            SMA.OnFileCreate((string)data[0],
                             (IntPtr)data[1]);
            break;

          case HookedFunction.SetFilePointer:
            SMA.OnFileSeek((IntPtr)data[0],
                           (UInt32)data[1]);
            break;

          case HookedFunction.WriteFile:
            var byteArr = (byte[])data[1];

            SMA.OnFileWrite((IntPtr)data[0],
                            byteArr,
                            (UInt32)data[2]);

            ArrayPool<byte>.Shared.Return(byteArr);
            break;

          case HookedFunction.CloseHandle:
            SMA.OnFileClose((IntPtr)data[0]);
            break;
        }
      }
      catch (Exception ex)
      {
        OnException(ex);
      }
    }

    private void KeepAlive()
    {
      try
      {
        while (!HasExited)
        {
          SMA.KeepAlive();

          for (int i = 0; i < 30 && !HasExited; i++)
            Thread.Sleep(1000);
        }
      }
      catch
      {
        // Ignored
      }

      HasExited = true;
    }

    private void Debug(string          str,
                       params object[] args)
    {
      SMA.Debug(str, args);
    }

    private void OnException<T>(T ex)
      where T : Exception
    {
      try
      {
        SentrySdk.CaptureException(ex);
        SMA?.OnException(ex);
      }
      catch { /* ignored */ }
    }

    #endregion
  }
}
