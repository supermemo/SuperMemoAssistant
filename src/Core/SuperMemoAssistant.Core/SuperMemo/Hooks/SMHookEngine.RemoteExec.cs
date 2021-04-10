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
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Anotar.Serilog;
  using Nito.AsyncEx;
  using Process.NET.Assembly;
  using SMA.Hooks.Models;

  [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
  public partial class SMHookEngine
  {
    #region Properties & Fields - Non-Public

    private readonly ConcurrentDictionary<int, AsyncAutoResetEvent> _execIdEventMap = new ConcurrentDictionary<int, AsyncAutoResetEvent>();
    private readonly object                                         _execIdLock     = new object();

    private readonly ConcurrentDictionary<int, int> _execIdResultMap = new ConcurrentDictionary<int, int>();

    private int _execIdCounter;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    [LogToErrorOnException]
    public void SetExecutionResults(Dictionary<int, int> idResultMap)
    {
      foreach (var kvp in idResultMap)
      {
        if (_execIdEventMap.TryRemove(kvp.Key, out var @event) == false)
          continue;

        _execIdResultMap[kvp.Key] = kvp.Value;
        @event.Set();
      }
    }

    #endregion




    #region Methods

    private int InterlockedIdIncrement(int toAdd)
    {
      int ret;

      lock (_execIdLock)
      {
        ret            =  _execIdCounter;
        _execIdCounter += toAdd;
      }

      return ret;
    }

    private static NativeExecutionParameters CreateExecParams(NativeMethod        method,
                                                              bool                shouldHoldMainThread,
                                                              IEnumerable<object> parameters,
                                                              int                 idCounter)
    {
      return new NativeExecutionParameters
      {
        Id                   = idCounter,
        Type                 = InjectLibExecutionType.ExecuteOnMainThread,
        Method               = method,
        Parameters           = parameters,
        ShouldHoldMainThread = shouldHoldMainThread
      };
    }

    private NativeExecutionParameters CreateExecParams(NativeMethod        method,
                                                       bool                shouldHoldMainThread,
                                                       IEnumerable<object> parameters,
                                                       out int             lastId)
    {
      lastId = InterlockedIdIncrement(1);

      return CreateExecParams(
        method,
        shouldHoldMainThread,
        parameters,
        lastId);
    }

    private IEnumerable<NativeExecutionParameters> CreateExecParams<TMeta>(
      IEnumerable<SMExecRequest<TMeta>> execRequests,
      out int                    lastId)
    {
      int count  = execRequests.Count();
      int iterId = InterlockedIdIncrement(count);

      lastId = iterId + count - 1;

      var ret = new List<NativeExecutionParameters>(count);

      foreach (var er in execRequests)
      {
        er.ExecId = iterId;
        var nep = CreateExecParams(
          er.Method,
          er.ShouldHoldMainThread,
          er.Parameters,
          iterId++);

        ret.Add(nep);
      }

      return ret;
    }

    private async Task<bool> ExecuteOnMainThreadAsync<T>(
      Action<T> requestExec,
      T         requestPayload,
      int       eventId)
    {
      var @event = _execIdEventMap[eventId] = new AsyncAutoResetEvent();
      var cts    = new CancellationTokenSource(AssemblyFactory.ExecutionTimeout);

      requestExec(requestPayload);

      try
      {
        await @event.WaitAsync(cts.Token).ConfigureAwait(false);

        return true;
      }
      catch (TaskCanceledException)
      {
        // TODO: Tiny chance of race condition
        _execIdEventMap.TryRemove(eventId, out _);

        return false;
      }
      finally
      {
        cts.Dispose();
      }
    }

    [LogToErrorOnException]
    public async Task<IEnumerable<SMExecResult<TMeta>>> ExecuteOnMainThreadAsync<TMeta>(
      IEnumerable<SMExecRequest<TMeta>> execRequests)
    {
      var execParams = CreateExecParams(execRequests, out int lastId);

      if (await ExecuteOnMainThreadAsync(_injectLib.RequestExecution,
                                         execParams,
                                         lastId).ConfigureAwait(false))
        return Enumerable.Empty<SMExecResult<TMeta>>();

      var ret = new List<SMExecResult<TMeta>>();

      foreach (var execRequest in execRequests)
      {
        if (_execIdResultMap.TryRemove(execRequest.ExecId, out int result) == false)
          result = int.MinValue;

        var execResult = new SMExecResult<TMeta>(result, execRequest);

        ret.Add(execResult);
      }

      return ret;
    }

    [LogToErrorOnException]
    public async Task<SMExecResult<TMeta>> ExecuteOnMainThreadAsync<TMeta>(
      SMExecRequest<TMeta> execRequest)
    {
      var res = await ExecuteOnMainThreadAsync(
        execRequest.Method,
        execRequest.ShouldHoldMainThread,
        execRequest.Parameters).ConfigureAwait(false);

      return new SMExecResult<TMeta>(res, execRequest);
    }

    [LogToErrorOnException]
    public async Task<int> ExecuteOnMainThreadAsync(
      NativeMethod method,
      bool         shouldHoldMainThread,
      IEnumerable<object>     parameters)
    {
      var execParams = CreateExecParams(
        method,
        shouldHoldMainThread,
        parameters,
        out int id);

      if (await ExecuteOnMainThreadAsync(
        _injectLib.RequestExecution,
        execParams,
        id).ConfigureAwait(false))
        return int.MinValue;

      return _execIdResultMap.TryRemove(execParams.Id, out int result)
        ? result
        : int.MinValue;
    }

    #endregion
  }
}
