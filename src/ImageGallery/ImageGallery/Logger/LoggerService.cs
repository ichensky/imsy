using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Logger
{
    public static class LoggerService
    {
        public static void Init(string logPath)
        {
            var configuration = new LoggingConfiguration();

            CreateLogFileTarget(configuration, NLog.LogLevel.Trace, logPath, Logger.log.ToString(), Logger.log.ToString().ToString(), 30);
            //CreateLogColoredConsoleTarget(configuration, NLog.LogLevel.Trace, Logger.log.ToString());

            NLog.LogManager.Configuration = configuration;
        }

        private static void CreateLogFileTarget(LoggingConfiguration config, NLog.LogLevel loglevel, string dirpath, string name, string filename, int days)
        {
            var layout = new NLog.Layouts.SimpleLayout("${longdate}|${level:uppercase=true}|${logger}|${callsite:className=true:fileName=false:includeSourcePath=false:methodName=true}|${message}");

            var target = new FileTarget(name)
            {
                FileNameKind = FilePathKind.Absolute,
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = days,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableFileDelete = true,
                FileName = Path.Combine(dirpath, filename + @".txt"),
                Layout = layout
            };

            config.AddTarget(target);
            config.LoggingRules.Add(new LoggingRule(name, loglevel, target));
        }

        //private static void CreateLogColoredConsoleTarget(LoggingConfiguration config, NLog.LogLevel loglevel, string name)
        //{
        //    var layout = new NLog.Layouts.SimpleLayout("${longdate}|${level:uppercase=true}|${logger}|${callsite:className=true:fileName=false:includeSourcePath=false:methodName=true}|${message}");

        //    var target = new ColoredConsoleTarget(name)
        //    {
        //        Layout = layout,
        //    };

        //    config.AddTarget(target);
        //    config.LoggingRules.Add(new LoggingRule(name, loglevel, target));
        //}
    }
}
