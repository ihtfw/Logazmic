namespace Logazmic.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;

    using Caliburn.Micro;

    using Logazmic.Annotations;
    using Logazmic.Core.Log;
    using Logazmic.Core.Reciever;
    using Logazmic.Services;
    using Logazmic.Settings;

    public class LogPaneViewModel : UpdatableScreen, IHandle<RefreshEvent>, IHandle<RefreshCheckEvent>, IDisposable
    {
        private CollectionViewSource collectionViewSource;

        private List<string> logSourceLeaves = new List<string>();

        private LogLevel minLogLevel;

        private bool scrollToEnd;

        private string searchString;

        public LogPaneViewModel([NotNull] ReceiverBase receiver)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            Receiver = receiver;
            LogMessages = new BindableCollection<LogMessage>();
            MinLogLevel = LogLevel.Trace;
            LogSourceRoot = new LogSource
                            {
                                Name = "Root"
                            };
            Messaging.Subscribe(this);
        }

        public bool AutoScroll { get; set; }

        public override string DisplayName
        {
            get
            {
                if (Receiver == null)
                {
                    return null;
                }

                return Receiver.DisplayName;
            }
            set { Receiver.DisplayName = value; }
        }

        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                Messaging.Publish(new RefreshEvent());
            }
        }

        public LogSource LogSourceRoot { get; private set; }

        public ReceiverBase Receiver { get; private set; }

        public BindableCollection<LogMessage> LogMessages { get; private set; }

        public LogMessage SelectedLogMessage { get; set; }

        public string ToolTip { get { return Receiver.Description; } }

        public LogLevel MinLogLevel
        {
            get { return minLogLevel; }
            set
            {
                minLogLevel = value;
                Messaging.Publish(new RefreshEvent());
            }
        }

        public CollectionViewSource CollectionViewSource
        {
            get
            {
                if (collectionViewSource == null)
                {
                    collectionViewSource = new CollectionViewSource();
                    collectionViewSource.Filter += LogMessagesViewSourceFilter;
                    var binding = new Binding
                                  {
                                      IsAsync = true,
                                      Source = this,
                                      Mode = BindingMode.OneWay,
                                      NotifyOnSourceUpdated = true,
                                      Path = new PropertyPath("LogMessages")
                                  };
                    BindingOperations.SetBinding(collectionViewSource, CollectionViewSource.SourceProperty, binding);
                }
                return collectionViewSource;
            }
        }

        public void Dispose()
        {
            if (Receiver != null)
            {
                Receiver.NewMessage -= OnNewMessage;
                Receiver.NewMessages -= OnNewMessages;
                Receiver.Terminate();
            }
        }

        public void Handle(RefreshCheckEvent message)
        {
            Update(true);
        }

        public void Handle(RefreshEvent message)
        {
            Update();
        }

        public async void Rename()
        {
            var newName = await DialogService.Current.ShowInputDialog("Rename", "Enter new name:");
            if (string.IsNullOrEmpty(newName))
            {
                return;
            }
            DisplayName = newName;
            LogazmicSettings.Instance.Save();
        }

        public void Clear()
        {
            LogMessages.Clear();
            Update();
        }

        public void Initialize()
        {
            try
            {
                Receiver.NewMessage += OnNewMessage;
                Receiver.NewMessages += OnNewMessages;
                Receiver.Initialize();

                Update(true);
            }
            catch (Exception e)
            {
                DialogService.Current.ShowErrorMessageBox(e);
                TryClose();
            }
        }

        private void LogMessagesViewSourceFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;

            var resultRow = e.Item as LogMessage;
            if (resultRow == null)
            {
                return;
            }

            if (resultRow.LogLevel < MinLogLevel)
            {
                return;
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resultRow.Message, SearchString, CompareOptions.IgnoreCase) < 0)
                {
                    return;
                }
            }

            if (!logSourceLeaves.Contains(resultRow.LastLoggerName))
            {
                return;
            }

            e.Accepted = true;
        }

        protected override void DoUpdate(bool full)
        {
            if (full)
            {
                logSourceLeaves = LogSourceRoot.Leaves().Where(l => l.IsChecked).Select(c => c.Name).Distinct().ToList();
            }

            Execute.OnUIThread(() =>
            {
                if (CollectionViewSource.View != null)
                {
                    CollectionViewSource.View.Refresh();
                }
            });
        }

        #region OnNewMessages

        private void OnNewMessages(object sender, LogMessage[] logMsgs)
        {
            Task.Factory.StartNew(() =>
            {
                lock (LogMessages)
                {
                    LogMessages.AddRange(logMsgs);
                    Array.ForEach(logMsgs, m => LogSourceRoot.Find(m.LoggerNames));
                    Update(true);
                }
            });
        }

        private void OnNewMessage(object sender, LogMessage logMsg)
        {
            Task.Factory.StartNew(() =>
            {
                lock (LogMessages)
                {
                    LogMessages.Add(logMsg);
                    LogSourceRoot.Find(logMsg.LoggerNames);
                    Update(true);
                }
            });
        }

        #endregion
    }
}