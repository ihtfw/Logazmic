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

    public class LogPaneViewModel : UpdatableScreen, IHandle<RefreshEvent>, IHandle<RefreshCheckEvent>, IDisposable
    {
        private CollectionViewSource collectionViewSource;

        private List<string> logSourceLeaves = new List<string>();

        private LogLevel minLogLevel;

        private string searchString;

        private bool scrollToEnd;

        public LogPaneViewModel([NotNull] AReceiver receiver)
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

            Task.Factory.StartNew(Init);
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

        public AReceiver Receiver { get; private set; }

        public BindableCollection<LogMessage> LogMessages { get; private set; }

        public LogMessage SelectedLogMessage { get; set; }

        public string ToolTip { get; set; }

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

                Receiver = null;
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

        public void Clear()
        {
            LogMessages.Clear();
            Update();
        }

        private void Init()
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
            LogMessages.AddRange(logMsgs);
            Array.ForEach(logMsgs, m => LogSourceRoot.Find(m.LoggerNames));
            Update(true);
        }

        private void OnNewMessage(object sender, LogMessage logMsg)
        {
            LogMessages.Add(logMsg);
            LogSourceRoot.Find(logMsg.LoggerNames);
            Update(true);
        }

        #endregion
    }
}