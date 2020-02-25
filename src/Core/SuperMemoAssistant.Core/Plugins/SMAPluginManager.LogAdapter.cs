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
// Modified On:  2020/02/25 11:45
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using PluginManager.Logger;
using Serilog;

namespace SuperMemoAssistant.Plugins
{
  public class PluginManagerLogAdapter : ILogAdapter
  {
    #region Properties Impl - Public

    /// <inheritdoc />
    public bool IsTraceEnabled => LogTo.IsVerboseEnabled;
    /// <inheritdoc />
    public bool IsDebugEnabled => LogTo.IsDebugEnabled;
    /// <inheritdoc />
    public bool IsInformationEnabled => LogTo.IsInformationEnabled;
    /// <inheritdoc />
    public bool IsWarningEnabled => LogTo.IsWarningEnabled;
    /// <inheritdoc />
    public bool IsErrorEnabled => LogTo.IsErrorEnabled;
    /// <inheritdoc />
    public bool IsFatalEnabled => LogTo.IsFatalEnabled;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public void Trace(string message)
    {
      Log.Logger?.Verbose(message);
    }

    /// <inheritdoc />
    public void Trace(string format, params object[] args)
    {
      Log.Logger?.Verbose(format, args);
    }

    /// <inheritdoc />
    public void Trace(Exception exception, string format, params object[] args)
    {
      Log.Logger?.Verbose(exception, format, args);
    }

    /// <inheritdoc />
    public void Debug(string message)
    {
      Log.Logger?.Debug(message);
    }

    /// <inheritdoc />
    public void Debug(string format, params object[] args)
    {
      Log.Logger?.Debug(format, args);
    }

    /// <inheritdoc />
    public void Debug(Exception exception, string format, params object[] args)
    {
      Log.Logger?.Debug(exception, format, args);
    }

    /// <inheritdoc />
    public void Information(string message)
    {
      Log.Logger?.Information(message);
    }

    /// <inheritdoc />
    public void Information(string format, params object[] args)
    {
      Log.Logger?.Information(format, args);
    }

    /// <inheritdoc />
    public void Information(Exception exception, string format, params object[] args)
    {
      Log.Logger?.Information(exception, format, args);
    }

    /// <inheritdoc />
    public void Warning(string message)
    {
      Log.Logger?.Warning(message);
    }

    /// <inheritdoc />
    public void Warning(string format, params object[] args)
    {
      Log.Logger?.Warning(format, args);
    }

    /// <inheritdoc />
    public void Warning(Exception exception, string format, params object[] args)
    {
      Log.Logger?.Warning(exception, format, args);
    }

    /// <inheritdoc />
    public void Error(string message)
    {
      Log.Logger?.Error(message);
    }

    /// <inheritdoc />
    public void Error(string format, params object[] args)
    {
      Log.Logger?.Error(format, args);
    }

    /// <inheritdoc />
    public void Error(Exception exception, string format, params object[] args)
    {
      Log.Logger?.Error(exception, format, args);
    }

    /// <inheritdoc />
    public void Fatal(string message)
    {
      Log.Logger?.Fatal(message);
    }

    /// <inheritdoc />
    public void Fatal(string format, params object[] args)
    {
      Log.Logger?.Fatal(format, args);
    }

    /// <inheritdoc />
    public void Fatal(Exception exception, string format, params object[] args)
    {
      Log.Logger?.Fatal(exception, format, args);
    }

    #endregion
  }
}
