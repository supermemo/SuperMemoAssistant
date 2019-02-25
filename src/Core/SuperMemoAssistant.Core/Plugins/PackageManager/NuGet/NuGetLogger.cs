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
// Modified On:  2019/01/20 08:26
// Modified By:  Alexis

#endregion




using System.Threading.Tasks;
using Anotar.Serilog;
using NuGet.Common;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet
{
  internal class NuGetLogger : ILogger
  {
    #region Methods Impl

    public void LogDebug(string data) => LogTo.Debug(data);

    public void LogVerbose(string data) => LogTo.Verbose(data);

    public void LogInformation(string data) => LogTo.Information(data);

    public void LogInformationSummary(string data) => LogTo.Information(data);

    public void LogMinimal(string data) => LogTo.Verbose(data);

    public void LogWarning(string data) => LogTo.Warning(data);

    public void LogError(string data) => LogTo.Error(data);

    public void Log(ILogMessage message) => Log(message.Level, message.Message);

    public void Log(LogLevel level,
                    string   data)
    {
      switch (level)
      {
        case LogLevel.Error:
          LogError(data);
          break;

        case LogLevel.Warning:
          LogWarning(data);
          break;

        case LogLevel.Information:
          LogInformation(data);
          break;

        case LogLevel.Debug:
          LogDebug(data);
          break;

        case LogLevel.Verbose:
          LogVerbose(data);
          break;

        case LogLevel.Minimal:
          LogMinimal(data);
          break;
      }
    }

    #endregion




#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task LogAsync(LogLevel level,
                               string   data) => Log(level, data);

    public async Task LogAsync(ILogMessage message) => Log(message.Level, message.Message);
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
  }
}
