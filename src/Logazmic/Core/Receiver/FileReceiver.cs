using System;
using System.IO;
using System.Linq;
using Logazmic.Core.Readers;

namespace Logazmic.Core.Receiver
{
    /// <summary>
    ///     This receiver watch a given file, like a 'tail' program, with one log event by line.
    ///     Ideally the log events should use the log4j XML Schema layout.
    /// </summary>
    public class FileReceiver : ReceiverBase
    {
        private readonly object _readingLock = new();
        private StreamReader _fileReader;

        private string _fileToWatch;

        private FileSystemWatcher _fileWatcher;

        private string _filename;

        private long _lastFileLength;
        private ILogStreamReader _logStreamReader;

        public string FileToWatch
        {
            get => _fileToWatch;
            set
            {
                _fileToWatch = value;
                DisplayName = Path.GetFileNameWithoutExtension(_fileToWatch);
            }
        }

        public override string Description => FileToWatch;

        #region AReceiver Members

        protected override void DoInitialize()
        {
            if (!File.Exists(FileToWatch))
            {
                throw new ApplicationException($"File \"{FileToWatch}\" does not exist.");
            }

            _logStreamReader = LogReaderFactory.LogStreamReader(LogFormat);
            _logStreamReader.BufferSize = 1 * 1024 * 1024; // 1 MB

            _fileReader = new StreamReader(new FileStream(FileToWatch, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            _lastFileLength = 0;

            // Tab was closed while reading
            if (!ReadFile()) return;

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
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }

            lock (_readingLock)
            {
                _fileReader?.Close();
                _fileReader = null;
            }

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

        /// <summary>
        /// </summary>
        /// <returns>True if success reading</returns>
        private bool ReadFile()
        {
            lock (_readingLock)
            {
                if (_fileReader == null) return false;
                if (_fileReader.BaseStream.Length == _lastFileLength) return true;

                // Seek to the last file length
                _fileReader.BaseStream.Seek(_lastFileLength, SeekOrigin.Begin);
            }


            int bytesRead;
            do
            {
                lock (_readingLock)
                {
                    if (_fileReader == null) return false;

                    var nextLogEvents = _logStreamReader.NextLogEvents(_fileReader.BaseStream, out bytesRead).ToList();
                    if (nextLogEvents.Any())
                        OnNewMessages(nextLogEvents);

                    // Update the last file length
                    _lastFileLength = _fileReader.BaseStream.Position;
                }
            } while (bytesRead > 0);

            return true;
        }
    }
}