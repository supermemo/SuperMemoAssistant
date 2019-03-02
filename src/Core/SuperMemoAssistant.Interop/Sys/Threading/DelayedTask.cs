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
// Created On:   2019/02/25 17:22
// Modified On:  2019/02/25 17:35
// Modified By:  Alexis

#endregion




using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Sys.Threading
{
  public class DelayedTask
  {
    #region Properties & Fields - Non-Public

    private readonly Action _callback;
    private readonly object _lock = new object();
    private readonly int    _refreshMs;

    private DateTime _next;
    private Task     _task;

    #endregion




    #region Constructors

    public DelayedTask(Action callback,
                       int    refreshMs = 50)
    {
      _callback  = callback;
      _refreshMs = refreshMs;
    }

    #endregion




    #region Methods

    public DelayedTask Trigger(int delayMs)
    {
      if (delayMs <= 0)
        throw new ArgumentException($"{nameof(delayMs)} must be greater than 0");

      lock (_lock)
      {
        _next = DateTime.Now.AddMilliseconds(delayMs);

        if (_task == null)
          _task = Task.Factory.StartNew(HandleAsync);

        return this;
      }
    }

    public void Cancel()
    {
      lock (_lock)
      {
        _next = DateTime.Now;
        _task = null;
      }
    }

    private void HandleAsync()
    {
      while (true)
      {
        while (DateTime.Now < _next)
          Thread.Sleep(_refreshMs);

        lock (_lock)
        {
          if (DateTime.Now < _next)
            continue;

          if (_task == null || _task.Id != Task.CurrentId)
            return;

          _task = null;
          break;
        }
      }

      _callback();
    }

    #endregion
  }
}
