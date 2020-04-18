using Logazmic.Core.Readers.Parsers;

namespace Logazmic.Core.Readers
{
    public class LogReaderFactory : ILogReaderFactory
    {
        public ILogStreamReader LogStreamReader(string logFormat)
        {
            var logParser = LogParser(logFormat);
            return new LogStreamReader(logParser);
        }

        public ILogParser LogParser(string logFormat)
        {
            switch (logFormat)
            {
                case LogFormats.Flat:
                    return new FlatLogParser();
            }

            return new Log4JParser();
        }

        public string GetLogFormatByFileExtension(string extension)
        {
            var ext = (extension ?? "").ToUpperInvariant();
            switch (ext)
            {
                case ".4J":
                case ".4JXML":
                case ".LOG4JXML":
                case ".LOG4J":
                    return LogFormats.Log4J;
                default:
                    return LogFormats.Flat;
            }
        }
    }
}