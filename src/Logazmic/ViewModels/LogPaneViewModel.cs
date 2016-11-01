namespace Logazmic.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Text;
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

        private string filterText;

        public LogPaneServices LogPaneServices { get; set; } = new LogPaneServices();

        public LogPaneViewModel([NotNull] ReceiverBase receiver)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            Receiver = receiver;
            LogMessages = new BindableCollection<LogMessage>();
            LogMessages.CollectionChanged += LogMessagesOnCollectionChanged;
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

        private void LogMessagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            NotifyOfPropertyChange(nameof(TotalLogMessages));
            NotifyOfPropertyChange(nameof(ShownLogMessages));
        }

        public bool AutoScroll { get; set; }

        public int TotalLogMessages => LogMessages.Count;

        public int ShownLogMessages => collectionViewSource?.View?.Cast<LogMessage>().Count() ?? 0;

        public override string DisplayName { get { return Receiver?.DisplayName; } set { Receiver.DisplayName = value; } }

        public string FilterText
        {
            get { return filterText; }
            set
            {
                filterText = value;
                Update();
            }
        }

        private bool ContainsCaseInsesetive(LogMessage logMessage)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(logMessage.Message, SearchText, CompareOptions.IgnoreCase) >= 0;
        }

        public string SearchText { get; set; }

        private void OnSearchTextChanged()
        {
            IEnumerable<LogMessage> searchCollection = LogMessages;
            if (SelectedLogMessage != null && ContainsCaseInsesetive(SelectedLogMessage))
            {
                var afterLastFound = LogMessages.SkipWhile(m => m != SelectedLogMessage).Skip(1); // Serach from last found
                searchCollection = afterLastFound.Concat(LogMessages);
            }

            SelectedLogMessage = searchCollection.FirstOrDefault(ContainsCaseInsesetive);
            if (SelectedLogMessage != null)
            {
                ScrollIntoSelected(true);
            }
        }

        public LogSource LogSourceRoot { get; private set; }

        public ReceiverBase Receiver { get; private set; }

        public BindableCollection<LogMessage> LogMessages { get; private set; }

        public LogMessage SelectedLogMessage { get; set; }

        private LogSource selectedLogSource;

        private void OnSelectedLogMessageChanged()
        {
            if (SelectedLogMessage == null)
            {
                if (selectedLogSource != null)
                {
                    selectedLogSource.IsSelected = false;
                }
                selectedLogSource = null;
                return;
            }

            selectedLogSource = LogSourceRoot.Find(SelectedLogMessage.LoggerNames);
            if (selectedLogSource != null)
            {
                selectedLogSource.IsSelected = true;
            }
            //SelectedLogMessage.LastLoggerName
        }

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

        public bool CanSyncWithSelectedItem { get { return SelectedLogMessage != null; } }

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

        public void CopyCurrentToClipboard()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var row in CollectionViewSource.View.OfType<LogMessage>())
                {
                    sb.AppendLine(row.MessageSingleLine);
                }
                Clipboard.SetDataObject(sb.ToString());
            }
            catch (Exception e)
            {
                DialogService.Current.ShowErrorMessageBox(e);
            }
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
                throw;
            }
        }

        private void LogMessagesViewSourceFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;

            var resultRow = e.Item as LogMessage;
            if (IsFiltered(resultRow))
                return;

            e.Accepted = true;
        }

        private bool IsFiltered(LogMessage logMessage)
        {
            if (logMessage == null)
            {
                return true;
            }

            if (logMessage.LogLevel < MinLogLevel.LogLevel)
            {
                return true;
            }

            if (!LogLevels.First(l => l.LogLevel == logMessage.LogLevel).IsEnabled)
            {
                return true;
            }

            foreach (var messageFilter in MessageFilters.Where(mf => mf.IsEnabled))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(logMessage.Message, messageFilter.Filter, CompareOptions.IgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(FilterText))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(logMessage.Message, FilterText, CompareOptions.IgnoreCase) < 0)
                {
                    return true;
                }
            }

            if (logSourceLeaves.All(l => l != logMessage.LoggerName))
            {
                return true;
            }
            return false;
        }

        protected override void DoUpdate(bool full)
        {
            if (full)
            {
                logSourceLeaves = LogSourceRoot.Leaves().Where(l => l.IsChecked).Select(c => c.FullName).Distinct().ToList();
            }

            Execute.OnUIThread(() =>
            {
                var prevSelected = SelectedLogMessage;
                CollectionViewSource.View?.Refresh();
                if (CollectionViewSource.View != null && prevSelected != null)
                {
                    if (IsFiltered(prevSelected))
                    {
                        var index = LogMessages.IndexOf(prevSelected);
                        if (index > 0)
                        {
                            foreach (var closestPrevMessage in LogMessages.Take(index).Reverse())
                            {
                                if (!IsFiltered(closestPrevMessage))
                                {
                                    SelectedLogMessage = closestPrevMessage;
                                    break;
                                }
                            }
                        }
                    }
                }
                NotifyOfPropertyChange(nameof(ShownLogMessages));
            });

            ScrollIntoSelected();
        }

        public void ScrollIntoSelected(bool forced = false)
        {
            if (forced || !AutoScroll)
            {
                ((dynamic)GetView())?.ScrollIntoSelected();
            }
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

        public void Handle(RefreshCheckEvent message)
        {
            Update(true);
        }

        public void Handle(RefreshEvent message)
        {
            Update();
        }

        public void Dispose()
        {
            if (Receiver != null)
            {
                Receiver.NewMessage -= OnNewMessage;
                Receiver.NewMessages -= OnNewMessages;
                Receiver.Terminate();
            }

            LogMessages.CollectionChanged -= LogMessagesOnCollectionChanged;
        }

        public void FindNext()
        {
            OnSearchTextChanged();
        }
    }
}