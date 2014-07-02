namespace Logazmic.Core.Log
{
    using System;

    public sealed class LogLevels
    {
        private static LogLevels _instance;

        public readonly LogLevelInfo InvalidLogLevel;
        public readonly LogLevelInfo[] LogLevelInfos;

        private LogLevels()
        {
            InvalidLogLevel = new LogLevelInfo(LogLevel.None);

            LogLevelInfos = new[]
                            {
                                new LogLevelInfo(LogLevel.Trace, "Trace", 10000, 0, 10000),
                                new LogLevelInfo(LogLevel.Debug, "Debug", 30000, 10001, 30000),
                                new LogLevelInfo(LogLevel.Info, "Info", 40000, 30001, 40000),
                                new LogLevelInfo(LogLevel.Warn, "Warn", 60000, 40001, 60000),
                                new LogLevelInfo(LogLevel.Error, "Error",  70000, 60001, 70000),
                                new LogLevelInfo(LogLevel.Fatal, "Fatal", 110000, 70001, 110000)
                            };
        }

        internal static LogLevels Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LogLevels();
                return _instance;
            }
        }

        internal LogLevelInfo this[int level]
        {
            get
            {
                if ((level < (int)LogLevel.Trace) || (level > (int)LogLevel.Fatal))
                    return InvalidLogLevel;
                return LogLevelInfos[level];
            }
        }

        internal LogLevelInfo this[LogLevel logLevel]
        {
            get
            {
                var level = (int)logLevel;
                if ((level < (int)LogLevel.Trace) || (level > (int)LogLevel.Fatal))
                    return InvalidLogLevel;
                return LogLevelInfos[level];
            }
        }

        internal LogLevelInfo this[string level]
        {
            get
            {
                foreach (LogLevelInfo info in LogLevelInfos)
                {
                    if (info.Name.Equals(level, StringComparison.InvariantCultureIgnoreCase))
                        return info;
                }
                return InvalidLogLevel;
            }
        }

    }
}