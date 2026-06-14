using System;
using JetBrains.Annotations;
using Logazmic.Core.Log;
using NLog;
using NLogLogger = NLog.Logger;

namespace Logazmic.Core.Readers.Parsers
{
    public static class LogParserExtensions
    {
        private static readonly NLogLogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Parse LOG4JXml from string and return default LogMessage on exception
        /// </summary>
        [NotNull]
        public static LogMessage TryParseLogEvent(this ILogParser logParser, string logEvent, string defaultLogger)
        {
            try
            {
                return logParser.ParseLogEvent(logEvent);
            }
            catch (Exception e)
            {
                Logger.Trace(e, "Failed to parse log event. DefaultLogger={0}, Parser={1}", defaultLogger, logParser.GetType().Name);
                return new LogMessage
                {
                    // Create a simple log message with some default values
                    LoggerName = defaultLogger,
                    ThreadName = "NA",
                    Message = logEvent,
                    TimeStamp = DateTime.Now,
                    LogLevel = Logazmic.Core.Log.LogLevel.Info,
                    ExceptionString = e.Message
                };
            }
        }
    }
}
