namespace Logazmic.Core.Reciever
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Logazmic.Core.Log;

    /// <summary>
    ///     This receiver watch a given file, like a 'tail' program, with one log event by line.
    ///     Ideally the log events should use the log4j XML Schema layout.
    /// </summary>
    public class FileReceiver : ReceiverBase
    {
        public enum FileFormatEnums
        {
            Log4jXml,

            Flat,
        }

        private FileFormatEnums _fileFormat;

        private string _fullLoggerName;

        private StreamReader fileReader;

        private FileSystemWatcher fileWatcher;

        private string filename;

        private long lastFileLength;

        public FileReceiver(string path)
        {
            FileToWatch = path;
            DisplayName = Path.GetFileNameWithoutExtension(path);;
        }

        public string FileToWatch { get; private set; }

        public FileFormatEnums FileFormat { get { return _fileFormat; } set { _fileFormat = value; } }

        #region IReceiver Members

        protected override void DoInitilize()
        {
            fileReader =
                new StreamReader(new FileStream(FileToWatch, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            lastFileLength = 0;

            string path = Path.GetDirectoryName(FileToWatch);
            filename = Path.GetFileName(FileToWatch);
            fileWatcher = new FileSystemWatcher(path, filename);
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            fileWatcher.Changed += OnFileChanged;
            fileWatcher.EnableRaisingEvents = true;
        }

        public override void Terminate()
        {
            if (fileWatcher != null)
            {
                fileWatcher.EnableRaisingEvents = false;
                fileWatcher.Changed -= OnFileChanged;
                fileWatcher = null;
            }

            if (fileReader != null)
            {
                fileReader.Close();
            }
            fileReader = null;

            lastFileLength = 0;
        }

        public override void Attach(ILogMessageNotifiable notifiable)
        {
            base.Attach(notifiable);
            ReadFile();
        }

        #endregion

        private void Restart()
        {
            Terminate();
            Initialize();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            ReadFile();
        }

        private void ReadFile()
        {
            if ((fileReader == null) || (fileReader.BaseStream.Length == lastFileLength))
            {
                return;
            }

            // Seek to the last file length
            fileReader.BaseStream.Seek(lastFileLength, SeekOrigin.Begin);

            // Get last added lines
            string line;
            var sb = new StringBuilder();
            var logMsgs = new List<LogMessage>();

            while ((line = fileReader.ReadLine()) != null)
            {
                if (_fileFormat == FileFormatEnums.Flat)
                {
                    var logMsg = new LogMessage();
                    logMsg.LoggerName = _fullLoggerName;
                    logMsg.ThreadName = "NA";
                    logMsg.Message = line;
                    logMsg.TimeStamp = DateTime.Now;
                    logMsg.Level = LogLevels.Instance[LogLevel.Info];

                    logMsgs.Add(logMsg);
                }
                else
                {
                    sb.AppendLine(line);

                    // This condition allows us to process events that spread over multiple lines
                    if (line.Contains("</log4j:event>"))
                    {
                        LogMessage logMsg = ReceiverUtils.ParseLog4JXmlLogEvent(sb.ToString(), _fullLoggerName);
                        logMsgs.Add(logMsg);
                        sb = new StringBuilder();
                    }
                }
            }

            // Notify the UI with the set of messages
            Notifiable.Notify(logMsgs.ToArray());

            // Update the last file length
            lastFileLength = fileReader.BaseStream.Position;
        }
    }
}