using System.Linq;
using Logazmic.Core.Readers;

namespace Logazmic.Core.Reciever
{
    using System;
    using System.IO;

    /// <summary>
    ///     This receiver watch a given file, like a 'tail' program, with one log event by line.
    ///     Ideally the log events should use the log4j XML Schema layout.
    /// </summary>
    public class FileReceiver : ReceiverBase
    {
        private StreamReader _fileReader;

        private string _fileToWatch;

        private FileSystemWatcher _fileWatcher;

        private string _filename;

        private long _lastFileLength;
        private ILogStreamReader _logStreamReader;

        public FileReceiver()
        {
        }

        public string FileToWatch
        {
            get => _fileToWatch;
            set
            {
                _fileToWatch = value;
                DisplayName = Path.GetFileNameWithoutExtension(_fileToWatch);
            }
        }

        public override string Description { get { return FileToWatch; } }
        
        #region AReceiver Members

        protected override void DoInitialize()
        {
            if (!File.Exists(FileToWatch))
            {
                throw new ApplicationException(string.Format("File \"{0}\" does not exist.", FileToWatch));
            }

            _logStreamReader = LogReaderFactory.LogStreamReader(LogFormat);
            _logStreamReader.BufferSize = 1 * 1024 * 1024; // 1 MB

            _fileReader = new StreamReader(new FileStream(FileToWatch, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            _lastFileLength = 0;

            ReadFile();

            string path = Path.GetDirectoryName(FileToWatch);
            _filename = Path.GetFileName(FileToWatch);
            _fileWatcher = new FileSystemWatcher(path, _filename)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.EnableRaisingEvents = true;
        }

        public override void Terminate()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Changed -= OnFileChanged;
                _fileWatcher = null;
            }

            _fileReader?.Close();
            _fileReader = null;

            _lastFileLength = 0;
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
            if ((_fileReader == null) || (_fileReader.BaseStream.Length == _lastFileLength))
            {
                return;
            }

            // Seek to the last file length
            _fileReader.BaseStream.Seek(_lastFileLength, SeekOrigin.Begin);

            int bytesRead;
            do
            {
                var nextLogEvents = _logStreamReader.NextLogEvents(_fileReader.BaseStream, out bytesRead).ToList();
                if (nextLogEvents.Any())
                    OnNewMessages(nextLogEvents);

                // Update the last file length
                _lastFileLength = _fileReader.BaseStream.Position;
            } while (bytesRead > 0);
        }
    }
}