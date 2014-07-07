namespace Logazmic.Core.Log
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class LogMessage
    {
        /// <summary>
        /// The Line Number of the Log Message
        /// </summary>
        public ulong SequenceNr { get; set; }

        /// <summary>
        /// Logger Name.
        /// </summary>
        public string LoggerName { get { return loggerName; }
            set
            {
                loggerName = value;
                LoggerNames = loggerName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                LastLoggerName = LoggerNames.LastOrDefault();
            } }

        public IReadOnlyList<string> LoggerNames { get; private set; }

        public string LastLoggerName { get; private set; }

        public LogLevel LogLevel { get; set; }
        
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

        private string loggerName;

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
    }
}