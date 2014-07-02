namespace Logazmic.Core.Log
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class LogMessage
    {
        private List<string> loggerNames;

        /// <summary>
        /// The Line Number of the Log Message
        /// </summary>
        public ulong SequenceNr { get; set; }

        /// <summary>
        /// Logger Name.
        /// </summary>
        public string LoggerName { get; set; }

        public IReadOnlyList<string> LoggerNames
        {
            get
            {
                if (loggerNames == null)
                {
                    loggerNames = LoggerName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                return loggerNames;
            }
        }

        public string LastLoggerName
        {
            get { return LoggerNames.LastOrDefault(); }
        }

        public LogLevel LogLevel
        {
            get
            {
                if (Level == null)
                {
                    return LogLevel.None;
                }
                return Level.Level;
            }
        }

        /// <summary>
        /// Log Level.
        /// </summary>
        public LogLevelInfo Level { get; set; }

        /// <summary>
        /// Log Message.
        /// </summary>
        public string Message { get; set; }

        public string MessageSingleLine
        {
            get
            {
                if (messageSingleLine == null)
                {
                    messageSingleLine = Message.Replace('\n', ' ').Replace('\r', ' ');
                }
                return messageSingleLine;
            }
        }

        /// <summary>
        /// Thread Name.
        /// </summary>
        public string ThreadName { get; set; }

        /// <summary>
        /// Time Stamp.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Properties collection.
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        private string messageSingleLine;

        /// <summary>
        /// An exception message to associate to this message.
        /// </summary>
        public string ExceptionString { get; set; }

        /// <summary>
        /// The CallSite Class
        /// </summary>
        public string CallSiteClass { get; set; }


        /// <summary>
        /// The CallSite Method in which the Log is made
        /// </summary>
        public string CallSiteMethod { get; set; }

        /// <summary>
        /// The Name of the Source File
        /// </summary>
        public string SourceFileName { get; set; }

        /// <summary>
        /// The Line of the Source File
        /// </summary>
        public uint SourceFileLineNr { get; set; }

        public void CheckNull()
        {
            if (string.IsNullOrEmpty(LoggerName))
                LoggerName = "Unknown";
            if (string.IsNullOrEmpty(Message))
                Message = "Unknown";
            if (string.IsNullOrEmpty(ThreadName))
                ThreadName = string.Empty;
            if (string.IsNullOrEmpty(ExceptionString))
                ExceptionString = string.Empty;
            if (string.IsNullOrEmpty(ExceptionString))
                ExceptionString = string.Empty;
            if (string.IsNullOrEmpty(CallSiteClass))
                CallSiteClass = string.Empty;
            if (string.IsNullOrEmpty(CallSiteMethod))
                CallSiteMethod = string.Empty;
            if (string.IsNullOrEmpty(SourceFileName))
                SourceFileName = string.Empty;
            if (Level == null)
                Level = LogLevels.Instance[(LogLevel.Error)];
        }
    }
}