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
// Created On:   2020/01/22 09:58
// Modified On:  2020/01/22 10:16
// Modified By:  Alexis

#endregion




using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Anotar.Serilog;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Services.Configuration;

namespace SuperMemoAssistant.Services.IO.Logger
{
  public sealed class Logger
  {
    #region Properties & Fields - Non-Public

    private LoggingLevelSwitch LevelSwitch { get; set; }
    private bool IsLogFirstChangeRegistered {get;set;}= false;

    #endregion




    #region Constructors

    public Logger(LoggerCfg config, LoggingLevelSwitch levelSwitch)
    {
      Config      = config;
      LevelSwitch = levelSwitch;

      RegisterExceptionLoggers();

      LogTo.Debug("Logger initialized");
    }

    #endregion




    #region Properties & Fields - Public

    public LoggerCfg Config { get; private set; }

    #endregion




    #region Methods

    public void Shutdown()
    {
      try
      {
        Log.CloseAndFlush();
      }
      catch
      {
        /* Ignore */
      }
    }

    /// <summary>
    /// Update logger settings with new <see cref="Config"/> parameters.
    /// </summary>
    public void ReloadConfig()
    {
      SetMinimumLevel(Config.LogLevel);

      if (Config.LogFirstChanceExceptions)
        RegisterFirstChanceExceptionLogger();

      else
        UnregisterFirstChanceExceptionLogger();
    }

    internal async Task ReloadConfigFromFile(ConfigurationServiceBase cfgService)
    {
      Config = await cfgService.Load<LoggerCfg>();

      ReloadConfig();
    }

    public LogEventLevel SetMinimumLevel(LogEventLevel level)
    {
      var oldLevel = LevelSwitch.MinimumLevel;
      LevelSwitch.MinimumLevel = level;

      LogTo.Information($"Logging level changed from {oldLevel.Name()} to {level.Name()}");

      return oldLevel;
    }

    private void RegisterExceptionLoggers()
    {
      AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

      if (Application.Current != null)
        Application.Current.DispatcherUnhandledException += LogDispatcherUnhandledException;

      if (Config.LogFirstChanceExceptions)
        RegisterFirstChanceExceptionLogger();

      LogTo.Debug($"Exception loggers registered (first-chance logging: {(Config.LogFirstChanceExceptions ? "on" : "off")})");
    }

    private void RegisterFirstChanceExceptionLogger()
    {
      if (IsLogFirstChangeRegistered)
        return;

      IsLogFirstChangeRegistered = true;
      AppDomain.CurrentDomain.FirstChanceException += LogFirstChanceException;
    }

    private void UnregisterFirstChanceExceptionLogger()
    {
      if (IsLogFirstChangeRegistered == false)
        return;

      IsLogFirstChangeRegistered = false;
      AppDomain.CurrentDomain.FirstChanceException -= LogFirstChanceException;
    }

    private void LogUnhandledException(object _, UnhandledExceptionEventArgs e)
    {
      if (e.ExceptionObject is Exception ex)
      {
        var msg = "Unhandled exception" + (e.IsTerminating ? ", terminating" : string.Empty);

        if (e.IsTerminating)
          LogTo.Fatal(ex, msg);

        else
          LogTo.Error(ex, msg);
      }

      if (e.IsTerminating)
        Shutdown();
    }

    private void LogDispatcherUnhandledException(object _, DispatcherUnhandledExceptionEventArgs e)
    {
      LogTo.Fatal(e.Exception, "Unhandled exception");

      if (e.Handled == false)
        Shutdown();
    }

    private void LogFirstChanceException(object _, FirstChanceExceptionEventArgs e)
    {
      LogTo.Error(e.Exception, "First chance exception");
    }
    
    // See https://github.com/Fody/Anotar/issues/114
    public static void ReloadAnotarLogger<T>()
    {
      ReloadAnotarLogger(typeof(T));
    }
    
    // See https://github.com/Fody/Anotar/issues/114
    public static void ReloadAnotarLogger(Type classType)
    {
      FieldInfo field;
      
      if ((field = classType.GetField("AnotarLogger", BindingFlags.NonPublic | BindingFlags.Static)) != null)
      {
        var logger = Log.ForContext(classType);
        field.SetValue(null, logger);
      }
    }

    #endregion
  }
}
