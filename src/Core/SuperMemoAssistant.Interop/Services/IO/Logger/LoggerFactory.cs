using System;
using Anotar.Serilog;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Services.IO.Logger
{
  /*
   * This is separate from Logger due to race condition with Fody's Anotar.Serilog
   * See https://github.com/Fody/Anotar/issues/114
   */
  public static class LoggerFactory
  {
    public delegate LoggerConfiguration LoggerConfigPredicate(LoggerConfiguration config);

    private const string OutputFormat = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static Logger Create(
      string                appName,
      ConfigurationServiceBase sharedConfig,
      LoggerConfigPredicate configPredicate = null)
    {
      if (Svc.Logger != null)
        throw new NotSupportedException();

      var config = LoadConfig(sharedConfig);
      var levelSwitch = new LoggingLevelSwitch(config.LogLevel);

      var loggerConfig = new LoggerConfiguration()
        .MinimumLevel.ControlledBy(levelSwitch)
        .Enrich.WithExceptionDetails()
        .Enrich.WithDemystifiedStackTraces()
        .WriteTo.Debug(outputTemplate: OutputFormat)
        .WriteTo.Async(
          a =>
            a.RollingFile(
              GetLogFilePath(appName).FullPath,
              fileSizeLimitBytes: 5242880, // Math.Max(ConfigMgr.AppConfig.LogMaxSize, 5242880),
              retainedFileCountLimit: 7,
              shared: false,
              outputTemplate: OutputFormat
            ));
      //.WriteTo.File(
      //  GetLogFilePath(appName).FullPath,
      //  outputTemplate: OutputFormat);
      //.WriteTo.RollingFile(
      //  GetLogFilePath(appName).FullPath,
      //  fileSizeLimitBytes: 5242880,
      //  retainedFileCountLimit: 7,
      //  shared: false,
      //  outputTemplate: OutputFormat
      //);

      if (configPredicate != null)
        loggerConfig = configPredicate(loggerConfig);

      Log.Logger = loggerConfig.CreateLogger();

      return new Logger(config, levelSwitch);
    }
    
    public static LoggerCfg LoadConfig(ConfigurationServiceBase sharedConfig)
    {
      try
      {
        return sharedConfig.Load<LoggerCfg>().Result ?? new LoggerCfg();
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

      var filePath = logDir.CombineFile($"{appName}-{{Date}}.log");

      return filePath;
    }
  }
}
