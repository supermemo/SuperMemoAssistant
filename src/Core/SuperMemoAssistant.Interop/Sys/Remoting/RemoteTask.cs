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
// Modified On:  2020/02/17 17:46
// Modified By:  Alexis

#endregion




using System;
using System.Threading.Tasks;
using Anotar.Serilog;

namespace SuperMemoAssistant.Sys.Remoting
{
  public partial class RemoteTask : MarshalByRefObject
  {
    #region Properties & Fields - Non-Public

    private readonly Task              _task;
    private readonly Action<Exception> _onExceptionHandler;
    private          bool              _calledCallback = false;

    private ActionProxy<Exception> _completedCallback;

    #endregion




    #region Constructors

    public RemoteTask(Task task, Action<Exception> onExceptionHandler = null)
    {
      _task               = task;
      _onExceptionHandler = onExceptionHandler;
      task.ContinueWith(OnTaskCompleted);
    }

    #endregion




    #region Methods

    public void SetCallback(ActionProxy<Exception> completedCallback)
    {
      _completedCallback = completedCallback;

      if (_task.IsCompleted)
        OnTaskCompleted(_task);
    }

    private void OnTaskCompleted(Task completedTask)
    {
      lock (_task)
      {
        if (_calledCallback || _completedCallback == null)
          return;

        if (completedTask.Exception != null)
        {
          if (_onExceptionHandler != null)
            _onExceptionHandler(completedTask.Exception);

          else
            LogTo.Warning(completedTask.Exception, "Exception caught");
        }

        try
        {
          _completedCallback.Invoke(completedTask.Exception);
        }
        catch
        {
          /* ignored */
        }

        _calledCallback = true;
      }
    }

    public static implicit operator RemoteTask(Task task)
    {
      return new RemoteTask(task);
    }

    #endregion
  }

  public class RemoteTask<T> : MarshalByRefObject
  {
    #region Properties & Fields - Non-Public

    private readonly Task<T>           _task;
    private readonly Action<Exception> _onExceptionHandler;
    private          bool              _calledCallback = false;

    private ActionProxy<T, Exception> _completedCallback;

    #endregion




    #region Constructors

    public RemoteTask(Task<T> task, Action<Exception> onExceptionHandler = null)
    {
      _task               = task;
      _onExceptionHandler = onExceptionHandler;
      task.ContinueWith(OnTaskCompleted);
    }

    #endregion




    #region Methods

    public void SetCallback(ActionProxy<T, Exception> completedCallback)
    {
      _completedCallback = completedCallback;

      if (_task.IsCompleted)
        OnTaskCompleted(_task);
    }

    private void OnTaskCompleted(Task<T> completedTask)
    {
      lock (_task)
      {
        if (_calledCallback || _completedCallback == null)
          return;

        T result = completedTask.Status == TaskStatus.RanToCompletion
          ? completedTask.Result
          : default;

        if (completedTask.Exception != null)
        {
          if (_onExceptionHandler != null)
            _onExceptionHandler(completedTask.Exception);

          else
            LogTo.Warning(completedTask.Exception, "Exception caught");
        }

        try
        {
          _completedCallback.Invoke(result, completedTask.Exception);
        }
        catch (Exception ex)
        {
          LogTo.Warning(ex, "Exception caught in RemoteTask");
        }

        _calledCallback = true;
      }
    }

    public static implicit operator RemoteTask<T>(Task<T> task)
    {
      return new RemoteTask<T>(task);
    }

    #endregion
  }
}
