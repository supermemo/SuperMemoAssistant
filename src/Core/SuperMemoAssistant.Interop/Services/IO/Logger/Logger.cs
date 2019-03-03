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
// Created On:   2019/02/25 22:02
// Modified On:  2019/03/02 12:59
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Threading;
using Anotar.Serilog;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Services.IO.Logger
{
  public delegate LoggerConfiguration LoggerConfigPredicate(LoggerConfiguration config);

  public class Logger
  {
    #region Constants & Statics

    protected const string OutputFormat = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";


    private static Logger _instance;
    private readonly ConfigurationService _configSvc;
    private LoggerCfg _config;

    public static  Logger Instance => _instance ?? (_instance = new Logger());

    #endregion




    #region Properties & Fields - Non-Public

    protected LoggingLevelSwitch LevelSwitch { get; set; }

    #endregion




    #region Constructors

    protected Logger()
    {
      _configSvc = new ConfigurationService(SMAFileSystem.SharedConfigDir);
    }

    #endregion




    #region Methods

    public void Initialize(
      string                appName,
      LoggerConfigPredicate configPredicate = null)
    {
      _config = LoadConfig();
      LevelSwitch = new LoggingLevelSwitch(_config.LogLevel);

      var config = new LoggerConfiguration()
                   .MinimumLevel.ControlledBy(LevelSwitch)
                   .Enrich.WithExceptionDetails()
                   .Enrich.WithDemystifiedStackTraces()
                   .WriteTo.Debug(outputTemplate: OutputFormat)
                   .WriteTo.Async(a =>
                                    a.RollingFile(
                                      GetLogFilePath(appName).FullPath,
                                      fileSizeLimitBytes: 5242880, // Math.Min(ConfigMgr.AppConfig.LogMaxSize, 26214400),
                                      retainedFileCountLimit: 7,
                                      shared: true,
                                      outputTemplate: OutputFormat
                                    ));

      if (configPredicate != null)
        config = configPredicate(config);

      Log.Logger = config.CreateLogger();

      LogTo.Debug("Logger initialized");

      RegisterExceptionLoggers();
    }

    public void Shutdown()
    {
      try
      {
        Log.CloseAndFlush();
      }
      catch
      {
        // Ignore
      }
    }

    public void ReloadConfig()
    {
      var newConfig = LoadConfig();

      SetMinimumLevel(newConfig.LogLevel);

      if (newConfig.LogFirstChanceExceptions != _config.LogFirstChanceExceptions)
      {
        if (newConfig.LogFirstChanceExceptions)
          RegisterFirstChanceExceptionLogger();

        else
          UnregisterFirstChanceExceptionLogger();
      }

      _config = newConfig;
    }

    public LogEventLevel SetMinimumLevel(LogEventLevel level)
    {
      var oldLevel = LevelSwitch.MinimumLevel;

      LevelSwitch.MinimumLevel = level;

      return oldLevel;
    }

    private void RegisterExceptionLoggers()
    {
      AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

      if (Application.Current != null)
        Application.Current.DispatcherUnhandledException += LogDispatcherUnhandledException;

      if (_config.LogFirstChanceExceptions)
        RegisterFirstChanceExceptionLogger();

      LogTo.Debug($"Exception loggers registered (first-chance logging: {(_config.LogFirstChanceExceptions ? "on" : "off")})");
    }

    private void RegisterFirstChanceExceptionLogger()
    {
      AppDomain.CurrentDomain.FirstChanceException += LogFirstChanceException;
    }

    private void UnregisterFirstChanceExceptionLogger()
    {
      AppDomain.CurrentDomain.FirstChanceException -= LogFirstChanceException;
    }

    private void LogUnhandledException(object _, UnhandledExceptionEventArgs e)
    {
      if (e.ExceptionObject is Exception ex)
      {
        var msg = "Unhandled exception" + (e.IsTerminating ? ", terminating" : string.Empty);
        
        LogTo.Error(ex, msg);
      }

      if (e.IsTerminating)
        Shutdown();
    }

    private void LogDispatcherUnhandledException(object _, DispatcherUnhandledExceptionEventArgs e)
    {
      LogTo.Error(e.Exception, "Unhandled exception");
    }

    private void LogFirstChanceException(object _, FirstChanceExceptionEventArgs e)
    {
      LogTo.Error(e.Exception, "First chance exception");
    }

    private LoggerCfg LoadConfig()
    {
      try
      {
        return _configSvc.Load<LoggerCfg>().Result ?? new LoggerCfg();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while loading logger config");

        return new LoggerCfg();
      }
    }

    private static FilePath GetLogFilePath(string appName)
    {
      var logDir = SMAFileSystem.LogDir;

      if (logDir.Exists() == false)
        logDir.Create();

      return logDir.CombineFile($"{appName}-{{Date}}.log");
    }

    #endregion
  }
}
