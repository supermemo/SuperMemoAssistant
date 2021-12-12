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
// Created On:   2020/03/29 00:20
// Modified On:  2020/03/29 06:02
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using System.Windows;
using Anotar.Serilog;

namespace SuperMemoAssistant
{
  /// <summary>Catches unhandled async exceptions https://github.com/Fody/AsyncErrorHandler</summary>
  public static class AsyncErrorHandler
  {
    #region Methods

    public static void HandleException(Exception exception)
    {
      LogTo.Error(exception, "Unhandled async exception");

      RethrowOnMainThread(exception);
    }

    [Conditional("DEBUG")]
    private static void RethrowOnMainThread(Exception ex)
    {
      Application.Current?.Dispatcher.InvokeAsync(() => throw ex);
    }

    #endregion
  }
}
