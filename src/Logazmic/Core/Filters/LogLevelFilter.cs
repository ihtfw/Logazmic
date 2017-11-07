using Logazmic.Core.Log;

namespace Logazmic.Core.Filters
{
    public class LogLevelFilter
    {
        public LogLevelFilter(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }

        public LogLevel LogLevel { get; }
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return $"{LogLevel} - {IsEnabled}";
        }
    }
}