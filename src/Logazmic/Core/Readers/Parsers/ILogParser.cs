using System.Collections.Generic;
using Logazmic.Core.Log;

namespace Logazmic.Core.Readers.Parsers
{
    public interface ILogParser
    {
        LogMessage ParseLogEvent(string logEvent);
        IEnumerable<LogEventParseItem> SplitToLogEventParseItems(string text);
    }
}