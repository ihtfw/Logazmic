using System;
using System.Collections.Generic;
using Logazmic.Core.Log;

namespace Logazmic.Core.Readers.Parsers
{
    public class FlatLogParser : ILogParser
    {
        public LogMessage ParseLogEvent(string logEvent)
        {
            return new LogMessage
            {
                LoggerName = LogFormats.Flat,
                ThreadName = "NA",
                Message = logEvent,
                TimeStamp = DateTime.Now,
                LogLevel = LogLevel.Info
            };
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