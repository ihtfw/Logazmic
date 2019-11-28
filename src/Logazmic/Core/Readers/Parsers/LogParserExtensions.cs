using System;
using JetBrains.Annotations;
using Logazmic.Core.Log;

namespace Logazmic.Core.Readers.Parsers
{
    public static class LogParserExtensions
    {
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
                return new LogMessage
                {
                    // Create a simple log message with some default values
                    LoggerName = defaultLogger,
                    ThreadName = "NA",
                    Message = logEvent,
                    TimeStamp = DateTime.Now,
                    LogLevel = LogLevel.Info,
                    ExceptionString = e.Message
                };
            }
        }
    }
}
