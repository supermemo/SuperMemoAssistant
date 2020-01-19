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
// Created On:   2019/02/23 16:23
// Modified On:  2019/02/23 16:55
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;

// ReSharper disable once CheckNamespace
namespace SuperMemoAssistant.Services.Sentry
{
  public abstract class SentrySMAPluginBase<TPlugin> : SMAPluginBase<TPlugin>
    where TPlugin : SentrySMAPluginBase<TPlugin>
  {
    #region Properties & Fields - Non-Public

    private readonly IDisposable _sentry;

    #endregion




    #region Constructors

    /// <inheritdoc />
    protected SentrySMAPluginBase(
      DebuggerAttachStrategy debuggerAttachStrategy = DebuggerAttachStrategy.Never)
      : base(debuggerAttachStrategy)
    {
      var pluginType = typeof(TPlugin);
      // ReSharper disable once VirtualMemberCallInConstructor
      var releaseName = $"{Name}@{pluginType.GetAssemblyVersion()}";

      _sentry = SentryEx.Initialize(releaseName);
    }

    public override void Dispose()
    {
      try
      {
        _sentry.Dispose();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while disposing Sentry instance");
      }

      base.Dispose();
    }

    #endregion




    #region Methods Impl

    protected override LoggerConfiguration ConfigureLogger(LoggerConfiguration loggerConfiguration)
    {
      return loggerConfiguration.LogToSentry();
    }

    #endregion
  }
}
