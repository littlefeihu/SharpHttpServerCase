using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Configuration;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace TechSvr.Utils
{
    public static class CustomRollingFileLogger
    {
        private static readonly ConcurrentDictionary<string, ILog> loggerContainer = new ConcurrentDictionary<string, ILog>();

        //默认配置  
        private const int MAX_SIZE_ROLL_BACKUPS = 20;
        private const string LAYOUT_PATTERN = "%newline记录时间：%date% 描述：%message%newline";
        private const string DATE_PATTERN = "yyyyMMdd";
        private const string MAXIMUM_FILE_SIZE = "2MB";
        private const string LEVEL = "ALL";

        public static ILog GetCustomLogger(string loggerName, string category = null, bool additivity = false)
        {
            return loggerContainer.GetOrAdd(loggerName, delegate (string name)
            {
                RollingFileAppender newAppender = GetNewFileApender(loggerName, GetFile(category, loggerName), MAX_SIZE_ROLL_BACKUPS, true, true, MAXIMUM_FILE_SIZE, RollingFileAppender.RollingMode.Composite,
                    DATE_PATTERN, LAYOUT_PATTERN);

                log4net.Repository.Hierarchy.Hierarchy repository = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
                Logger logger = repository.LoggerFactory.CreateLogger(repository, loggerName);
                logger.Hierarchy = repository;
                logger.Parent = repository.Root;
                logger.Level = GetLoggerLevel(LEVEL);
                logger.Additivity = additivity;
                logger.AddAppender(newAppender);
                logger.Repository.Configured = true;
                return new LogImpl(logger);
            });
        }

        //如果没有指定文件路径则在运行路径下建立 Log\{loggerName}.txt  
        private static string GetFile(string category, string loggerName)
        {
            if (string.IsNullOrEmpty(category))
            {
                return string.Format(@"Logs\{0}.txt", loggerName);
            }
            else
            {
                return string.Format(@"Logs\{0}\{1}.txt", category, loggerName);
            }
        }

        private static Level GetLoggerLevel(string level)
        {
            if (!string.IsNullOrEmpty(level))
            {
                switch (level.ToLower().Trim())
                {
                    case "debug":
                        return Level.Debug;

                    case "info":
                        return Level.Info;

                    case "warn":
                        return Level.Warn;

                    case "error":
                        return Level.Error;

                    case "fatal":
                        return Level.Fatal;
                }
            }
            return Level.Debug;
        }

        private static RollingFileAppender GetNewFileApender(string appenderName, string file, int maxSizeRollBackups, bool appendToFile = true, bool staticLogFileName = false, string maximumFileSize = "2MB", RollingFileAppender.RollingMode rollingMode = RollingFileAppender.RollingMode.Size, string datePattern = "yyyyMMdd\".txt\"", string layoutPattern = "%d [%t] %-5p %c  - %m%n")
        {
            RollingFileAppender appender = new RollingFileAppender
            {
                LockingModel = new FileAppender.MinimalLock(),
                Name = appenderName,
                File = file,
                AppendToFile = appendToFile,
                MaxSizeRollBackups = maxSizeRollBackups,
                MaximumFileSize = maximumFileSize,
                StaticLogFileName = staticLogFileName,
                RollingStyle = rollingMode,
                DatePattern = datePattern
            };
            PatternLayout layout = new PatternLayout(layoutPattern);
            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();
            return appender;
        }
    }
}
