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
// Created On:   2018/12/20 12:23
// Modified On:  2018/12/20 12:29
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using SuperMemoAssistant.Interop;

namespace SuperMemoAssistant.Services.IO
{
  public class Logger
  {
    #region Constants & Statics

    protected const string OutputFormat = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";


    private static Logger instance;
    public static  Logger Instance => instance ?? (instance = new Logger());

    #endregion




    #region Properties & Fields - Non-Public

    protected LoggingLevelSwitch LevelSwitch { get; set; }

    #endregion




    #region Constructors

    protected Logger() { }

    #endregion




    #region Methods

    public void Initialize()
    {
      LevelSwitch = new LoggingLevelSwitch(LogEventLevel.Debug); // ConfigMgr.AppConfig.LogMinLevel);

      Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.ControlledBy(LevelSwitch)
                   .Enrich.WithExceptionDetails()
                   .Enrich.WithDemystifiedStackTraces()
                   .WriteTo.Debug(outputTemplate: OutputFormat)
                   .WriteTo.Async(a =>
                                    a.RollingFile(
                                      GetLogFilePath(),
                                      fileSizeLimitBytes: 5242880, // Math.Min(ConfigMgr.AppConfig.LogMaxSize, 26214400),
                                      retainedFileCountLimit: 7,
                                      shared: true,
                                      outputTemplate: OutputFormat
                                    ))
                   .CreateLogger();
    }

    public static string GetLogFilePath()
    {
      string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

      return Path.Combine(appData,
                          SMConst.AppName,
                          "log-{Date}.txt");
    }

    public void SetMinimumLevel(LogEventLevel level)
    {
      LevelSwitch.MinimumLevel = level;
    }

    public void Shutdown()
    {
      Log.CloseAndFlush();
    }

    #endregion
  }
}
