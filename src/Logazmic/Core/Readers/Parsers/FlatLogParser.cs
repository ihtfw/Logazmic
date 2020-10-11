using System;
using System.Collections.Generic;
using System.Globalization;
using Logazmic.Core.Log;

namespace Logazmic.Core.Readers.Parsers
{
    public class FlatLogParser : ILogParser
    {
        public LogMessage ParseLogEvent(string logEvent)
        {
            var logMessage = ParseExactTemplateOrDefault(logEvent);
            if (logMessage != null)
            {
                return logMessage;
            }

            return new LogMessage
            {
                LoggerName = LogFormats.Flat,
                ThreadName = "NA",
                Message = logEvent,
                TimeStamp = DateTime.Now,
                LogLevel = LogLevel.Info
            };
        }

        /// <summary>
        /// Supported formats:
        /// 2020-09-03 18:14:10.103 +07:00 [INF] Request finished in 89.4299ms 304
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        private LogMessage ParseExactTemplateOrDefault(string logEvent)
        {
            var indexOfOpenBracket = logEvent.IndexOf(" [", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfOpenBracket < 1)
                return null;

            var indexOfCloseBracket = logEvent.IndexOf("] ", indexOfOpenBracket, StringComparison.InvariantCultureIgnoreCase);
            if (indexOfCloseBracket < 1)
                return null;

            var logLevelRaw = logEvent.Substring(indexOfOpenBracket + 2, indexOfCloseBracket - indexOfOpenBracket - 2);
            LogLevel logLevel = TryParseLogLevel(logLevelRaw);
            if (logLevel == LogLevel.None) return null;

            var timestampRaw = logEvent.Substring(0, indexOfOpenBracket);
            if (!DateTime.TryParseExact(timestampRaw, "yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
            {
                return null;
            }

            return new LogMessage
            {
                LoggerName = LogFormats.Flat,
                ThreadName = "NA",
                Message = logEvent.Substring(indexOfCloseBracket + 2),
                TimeStamp = timestamp,
                LogLevel = logLevel
            };
        }

        private static LogLevel TryParseLogLevel(string logEvent)
        {
            switch (logEvent.Trim().ToUpperInvariant())
            {
                case "TCE":
                case "TRC":
                case "TRACE":
                    return LogLevel.Trace;
                case "DBG":
                case "DEBUG":
                    return LogLevel.Debug;
                case "INF":
                case "INFO":
                case "INFORMATION":
                    return LogLevel.Info;
                case "WRN":
                case "WARN":
                case "WARNING":
                    return LogLevel.Warn;
                case "FTL":
                case "FATAL":
                    return LogLevel.Fatal;
                default:
                    return LogLevel.None;
            }
        }

        public IEnumerable<LogEventParseItem> SplitToLogEventParseItems(string text)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            var start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    var length = i - start;
                    if (length > 0)
                    {
                        yield return new LogEventParseItem(start, length);
                    }
                    start = i + 1;
                }
            }

            if (start < text.Length)
            {
                yield return new LogEventParseItem(start, text.Length - start);
            }
        }
    }
}