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

        private LogLevelViewModel minLogLevel;

        private string searchString;

        public LogPaneServices LogPaneServices { get; set; } = new LogPaneServices();

        public LogPaneViewModel([NotNull] ReceiverBase receiver)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            Receiver = receiver;
            LogMessages = new BindableCollection<LogMessage>();

            LogSourceRoot = new LogSource(LogPaneServices)
                            {
                                Name = "Root"
                            };

            LogLevels = new BindableCollection<LogLevelViewModel>
                        {
                            new LogLevelViewModel(LogPaneServices, LogLevel.Trace),
                            new LogLevelViewModel(LogPaneServices, LogLevel.Debug),
                            new LogLevelViewModel(LogPaneServices, LogLevel.Info),
                            new LogLevelViewModel(LogPaneServices, LogLevel.Warn),
                            new LogLevelViewModel(LogPaneServices, LogLevel.Error),
                            new LogLevelViewModel(LogPaneServices, LogLevel.Fatal),
                        };
            MinLogLevel = LogLevels.First();

            LogPaneServices.EventAggregator.Subscribe(this);
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
                Update();
            }
        }

        public LogSource LogSourceRoot { get; private set; }

        public ReceiverBase Receiver { get; private set; }

        public BindableCollection<LogMessage> LogMessages { get; private set; }

        public LogMessage SelectedLogMessage { get; set; }

        public string ToolTip { get { return Receiver.Description; } }

        public BindableCollection<LogLevelViewModel> LogLevels { get; }

        public LogLevelViewModel MinLogLevel
        {
            get { return minLogLevel; }
            set
            {
                minLogLevel = value;
                Update();
            }
        }

        public BindableCollection<MessageFilterViewModel> MessageFilters { get; } = new BindableCollection<MessageFilterViewModel>();

        public void AddMessageFilter(string messageFilter)
        {
            if (string.IsNullOrWhiteSpace(messageFilter))
            {
                return;
            }
            if (MessageFilters.Any(mf => mf.Filter == messageFilter))
            {
                return;
            }
            var messageFilterViewModel = new MessageFilterViewModel(LogPaneServices, messageFilter);
            MessageFilters.Add(messageFilterViewModel);
            Update();
        }

        public void RemoveMessageFilter(MessageFilterViewModel messageFilter)
        {
            if (messageFilter == null)
            {
                return;
            }

            MessageFilters.Remove(messageFilter);
            Update();
        }

        public event EventHandler SyncWithSelectedItemRequired;

        public bool CanSyncWithSelectedItem { get { return SelectedLogMessage != null; } }

        public void SyncWithSelectedItem()
        {
            OnSyncWithSelectedItemRequired();
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

            if (resultRow.LogLevel < MinLogLevel.LogLevel)
            {
                return;
            }

            if (!LogLevels.First(l => l.LogLevel == resultRow.LogLevel).IsEnabled)
            {
                return;
            }

            foreach (var messageFilter in MessageFilters.Where(mf => mf.IsEnabled))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resultRow.Message, messageFilter.Filter, CompareOptions.IgnoreCase) >= 0)
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resultRow.Message, SearchString, CompareOptions.IgnoreCase) < 0)
                {
                    return;
                }
            }

            if (!logSourceLeaves.Any(l => l == resultRow.LoggerName))
            {
                return;
            }

            e.Accepted = true;
        }

        protected override void DoUpdate(bool full)
        {
            if (full)
            {
                logSourceLeaves = LogSourceRoot.Leaves().Where(l => l.IsChecked).Select(c => c.FullName).Distinct().ToList();
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

        protected virtual void OnSyncWithSelectedItemRequired()
        {
            var handler = SyncWithSelectedItemRequired;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}