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

        private FileFormatEnums fileFormat;

        private StreamReader fileReader;

        private string fileToWatch;

        private FileSystemWatcher fileWatcher;

        private string filename;

        private long lastFileLength;

    
        public string FileToWatch
        {
            get { return fileToWatch; }
            set
            {
                fileToWatch = value;
                DisplayName = Path.GetFileNameWithoutExtension(fileToWatch);
            }
        }

        public override string Description { get { return FileToWatch; } }

        public FileFormatEnums FileFormat { get { return fileFormat; } set { fileFormat = value; } }

        #region AReceiver Members

        protected override void DoInitilize()
        {
            if (!File.Exists(FileToWatch))
            {
                throw new ApplicationException(string.Format("File \"{0}\" does not exist.", FileToWatch));
            }

            fileReader = new StreamReader(new FileStream(FileToWatch, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            lastFileLength = 0;

            string path = Path.GetDirectoryName(FileToWatch);
            filename = Path.GetFileName(FileToWatch);
            fileWatcher = new FileSystemWatcher(path, filename)
                          {
                              NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                          };
            fileWatcher.Changed += OnFileChanged;
            fileWatcher.EnableRaisingEvents = true;

            ReadFile();
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

        #endregion

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
                if (fileFormat == FileFormatEnums.Flat)
                {
                    var logMsg = new LogMessage
                                 {
                                     ThreadName = "NA",
                                     Message = line,
                                     TimeStamp = DateTime.Now,
                                     LogLevel = LogLevel.Info
                                 };

                    logMsgs.Add(logMsg);
                }
                else
                {
                    sb.AppendLine(line);

                    // This condition allows us to process events that spread over multiple lines
                    if (line.Contains("</log4j:event>"))
                    {
                        LogMessage logMsg = ReceiverUtils.ParseLog4JXmlLogEvent(sb.ToString(), null);
                        logMsgs.Add(logMsg);
                        sb = new StringBuilder();
                    }
                }
            }

            // Notify the UI with the set of messages
            OnNewMessages(logMsgs.ToArray());

            // Update the last file length
            lastFileLength = fileReader.BaseStream.Position;
        }
    }
}