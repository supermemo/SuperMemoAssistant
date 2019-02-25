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
// Created On:   2019/01/20 08:05
// Modified On:  2019/01/20 12:10
// Modified By:  Alexis

#endregion




using System;
using System.Xml.Linq;
using Anotar.Serilog;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet.Project
{
  /// <summary>Original from: https://github.com/Wyamio/Wyam/ Copyright (c) 2014 Dave Glick</summary>
  internal class NuGetProjectContext : INuGetProjectContext
  {
    #region Constructors

    /// <inheritdoc />
    public NuGetProjectContext(ISettings settings)
    {
      var nuGetLogger = new NuGetLogger();

      PackageExtractionContext = new PackageExtractionContext(
        PackageSaveMode.Defaultv3,
        XmlDocFileSaveMode.None,
        ClientPolicyContext.GetClientPolicy(settings, nuGetLogger),
        nuGetLogger);
    }

    #endregion




    #region Properties Impl - Public

    public PackageExtractionContext PackageExtractionContext { get; set; }

    public XDocument OriginalPackagesConfig { get; set; }

    public ISourceControlManagerProvider SourceControlManagerProvider => null;

    public ExecutionContext ExecutionContext => null;

    public NuGetActionType ActionType { get; set; }

    public Guid OperationId { get; set; }

    #endregion




    #region Methods Impl

    public void Log(MessageLevel    level,
                    string          message,
                    params object[] args)
    {
      switch (level)
      {
        case MessageLevel.Error:
          LogTo.Error(message, args);
          break;

        case MessageLevel.Warning:
          LogTo.Warning(message, args);
          break;

        case MessageLevel.Info:
          LogTo.Information(message, args);
          break;

        case MessageLevel.Debug:
          LogTo.Debug(message, args);
          break;
      }
    }
    
    public void Log(LogLevel level,
                    string   data)
    {
      switch (level)
      {
        case LogLevel.Error:
          LogTo.Error(data);
          break;

        case LogLevel.Warning:
          LogTo.Warning(data);
          break;

        case LogLevel.Information:
          LogTo.Information(data);
          break;

        case LogLevel.Debug:
          LogTo.Debug(data);
          break;
          
        case LogLevel.Verbose:
        case LogLevel.Minimal:
          LogTo.Verbose(data);
          break;
      }
    }

    public void Log(ILogMessage message)
    {
      Log(message.Level, message.Message);
    }

    public void ReportError(ILogMessage message)
    {
      Log(message.Level, message.Message);
    }

    public FileConflictAction ResolveFileConflict(string message) => FileConflictAction.Ignore;

    public void ReportError(string message)
    {
      LogTo.Error(message);
    }

    #endregion
  }
}
