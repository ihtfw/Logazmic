using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Logazmic.Core.Log;
using Logazmic.Core.Readers.Parsers;

namespace Logazmic.Core.Readers
{
    public class LogStreamReader : ILogStreamReader
    {
        private readonly ILogParser _logParser;
        private string _tail = string.Empty;
        private byte[] _buffer;

        public int MaxTailSize { get; set; } = 262144;

        public string DefaultLogger { get; set; } = "DefaultLogger";

        public LogStreamReader(ILogParser logParser)
        {
            _logParser = logParser;

            BufferSize = 10 * 1024;
        }

        public int BufferSize
        {
            get => _buffer.Length;
            set => _buffer = new byte[value];
        }

        public IEnumerable<LogMessage> NextLogEvents(Stream stream, out int bytesRead)
        {
            // tail > 256kb clean up
            if (!string.IsNullOrEmpty(_tail) && _tail.Length > MaxTailSize)
            {
                _tail = string.Empty;
            }
            
            bytesRead = stream.Read(_buffer, 0, _buffer.Length);
            if (bytesRead == 0)
                return Enumerable.Empty<LogMessage>();

            var text = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
            if (!string.IsNullOrEmpty(_tail))
            {
                text = _tail + text;
            }

            return ParseLogEvents(text);
        }

        private IEnumerable<LogMessage> ParseLogEvents(string text)
        {
            var maxIndex = 0;
            foreach (var logEventParseData in _logParser.SplitToLogEventParseItems(text))
            {
                maxIndex = Math.Max(maxIndex, logEventParseData.StartIndex + logEventParseData.Length);

                var data = text.Substring(logEventParseData.StartIndex, logEventParseData.Length);
                yield return _logParser.TryParseLogEvent(data, DefaultLogger);
            }

            _tail = text.Substring(maxIndex, text.Length - maxIndex);
        }
    }
}