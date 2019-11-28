using Logazmic.Core.Readers.Parsers;

namespace Logazmic.Core.Readers
{
    public interface ILogReaderFactory
    {
        ILogStreamReader LogStreamReader(string logFormat);
        ILogParser LogParser(string logFormat);
        string GetLogFormatByFileExtension(string extension);
    }
}