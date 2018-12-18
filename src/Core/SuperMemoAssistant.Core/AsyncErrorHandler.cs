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
// Created On:   2018/05/08 16:06
// Modified On:  2018/12/13 12:49
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
#if DEBUG
using System.Threading;
#endif

namespace SuperMemoAssistant
{
  /// <summary>Catches unhandled async exceptions https://github.com/Fody/AsyncErrorHandler</summary>
  public static class AsyncErrorHandler
  {
    #region Methods

    public static void HandleException(Exception exception)
    {
      LogTo.Error(exception,
                  "Unhandled async exception");

#if DEBUG
      SynchronizationContext.Current.Post(RethrowOnMainThread,
                                          exception);
#endif
    }

#if DEBUG
    private static void RethrowOnMainThread(object state)
    {
      Exception ex = state as Exception;

      // ReSharper disable once PossibleNullReferenceException
      throw ex;
    }
#endif

    #endregion
  }
}
