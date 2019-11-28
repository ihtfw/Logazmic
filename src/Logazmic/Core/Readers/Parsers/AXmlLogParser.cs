using System;
using System.Collections.Generic;
using System.Xml;
using Logazmic.Core.Log;

namespace Logazmic.Core.Readers.Parsers
{
    public abstract class AXmlLogParser : ILogParser
    {
        private readonly string _openEventTag;
        private readonly string _closeEventTag;

        protected AXmlLogParser(string eventTag)
        {
            _openEventTag = $"<{eventTag}";
            _closeEventTag = $"</{eventTag}>";
        }

        public LogMessage ParseLogEvent(string logEvent)
        {
            using (var reader = new XmlTextReader(logEvent, XmlNodeType.Element, GetXmlParserContext()))
                return ParseLogEvent(reader);
        }

        protected abstract LogMessage ParseLogEvent(XmlReader xmlTextReader);

        protected abstract XmlParserContext GetXmlParserContext();

        public IEnumerable<LogEventParseItem> SplitToLogEventParseItems(string text)
        {
            var startIndex = text.IndexOf(_openEventTag, StringComparison.InvariantCulture);
            if (startIndex < 0)
                yield break;

            var endIndex = text.IndexOf(_closeEventTag, startIndex, StringComparison.InvariantCulture);

            while (endIndex != -1)
            {
                //in case there was some problem with input data and we have two openings
                var nextOpenIndex = text.IndexOf(_openEventTag, startIndex + 1, StringComparison.InvariantCulture);
                if (nextOpenIndex > startIndex && nextOpenIndex < endIndex)
                {
                    startIndex = nextOpenIndex;
                }

                var length = endIndex + _closeEventTag.Length - startIndex;
                yield return new LogEventParseItem(startIndex, length);

                startIndex = text.IndexOf(_openEventTag, startIndex + length, StringComparison.InvariantCulture);
                if (startIndex < 0)
                    yield break;

                endIndex = text.IndexOf(_closeEventTag, startIndex, StringComparison.InvariantCulture);
            }
        }
    }
}