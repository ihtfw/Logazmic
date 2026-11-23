using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Logazmic.Core.Filters;
using Logazmic.Core.Receiver;
using Logazmic.Utils;
using Logazmic.ViewModels.Events;
using Logazmic.ViewModels.Filters;
using NLog;

namespace Logazmic.ViewModels
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;

    using Caliburn.Micro;
    
    using Core.Log;
    using Services;
    using Settings;

    public class LogPaneViewModel : UpdatableScreen, IHandle<RefreshEvent>, IDisposable
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private SourceFilterViewModel _selectedLogSource;
        private CollectionViewSource _collectionViewSource;

        private readonly IDisposable _searchTextChangedSubscriber;
        private readonly IDisposable _utcTimeChangedSubscriber;
        private readonly FilterLogic _filterLogic;
        public LogPaneViewModel([NotNull] ReceiverBase receiver)
        {
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            var filtersProfile = new FiltersProfile();
            LogMessages.CollectionChanged += LogMessagesOnCollectionChanged;

            ProfileFiltersViewModel = new ProfileFiltersViewModel(filtersProfile, LogPaneServices);
            ProfileFiltersViewModel.TimeFilter.SetUtcTime(LogazmicSettings.Instance.UtcTime);
            _filterLogic = new FilterLogic(filtersProfile);
            _utcTimeChangedSubscriber = LogazmicSettings.Instance.SubscribeToPropertyChanged(settings => settings.UtcTime, HandleUtcTimeChanged);
            ProfilesFiltersViewModel = new ProfilesFiltersViewModel(filtersProfile, LogPaneServices);
            _searchTextChangedSubscriber = ProfileFiltersViewModel.SubscribeToPropertyChanged(vm => vm.SearchText, OnSearchTextChanged);

            ProfilesFiltersViewModel.ActivateWith(this);

            LogPaneServices.EventAggregator.Subscribe(this);
        }


        private void HandleUtcTimeChanged()
        {
            Execute.OnUIThread(() =>
            {
                ProfileFiltersViewModel.TimeFilter.SetUtcTime(LogazmicSettings.Instance.UtcTime);
                Update();
            });
        }

        private void OnSearchTextChanged()
        {
            if (string.IsNullOrEmpty(ProfileFiltersViewModel.SearchText))
                return;
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

        private bool ContainsCaseInsesetive(LogMessage logMessage)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(logMessage.Message, ProfileFiltersViewModel.SearchText, CompareOptions.IgnoreCase) >= 0;
        }

        public LogPaneServices LogPaneServices { get; } = new LogPaneServices();
        public ProfileFiltersViewModel ProfileFiltersViewModel { get; }
        public ProfilesFiltersViewModel ProfilesFiltersViewModel { get; } 

        private void LogMessagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            NotifyOfPropertyChange(nameof(TotalLogMessages));
            NotifyOfPropertyChange(nameof(ShownLogMessages));
        }

        public bool AutoScroll { get; set; }
        
        public int TotalLogMessages => LogMessages.Count;

        public int ShownLogMessages => _collectionViewSource?.View?.Cast<LogMessage>().Count() ?? 0;

        public override string DisplayName { get => Receiver?.DisplayName;
            set => Receiver.DisplayName = value;
        }
        
        public ReceiverBase Receiver { get; }

        public BindableCollection<LogMessage> LogMessages { get; } =new BindableCollection<LogMessage>();

        public LogMessage SelectedLogMessage { get; set; }

        public string SearchSourceText { get; set; }

        private void OnSearchSourceTextChanged()
        {
            if (string.IsNullOrWhiteSpace(SearchSourceText))
            {
                OnSelectedLogMessageChanged();
                return;
            }

            var searchText = SearchSourceText.Trim();
            var path = FindBestSourcePath(searchText);
            var source = path == null ? null : path[path.Count - 1];

            if (path != null)
            {
                for (var i = 0; i < path.Count - 1; i++)
                {
                    path[i].IsExpanded = true;
                }
            }

            SelectLogSource(source);
        }

        private List<SourceFilterViewModel> FindBestSourcePath(string searchText)
        {
            var bestRank = int.MaxValue;
            List<SourceFilterViewModel> bestPath = null;
            var currentPath = new List<SourceFilterViewModel>();

            FindBestSourcePath(
                ProfileFiltersViewModel.SourceFilterRootViewModel.Children,
                searchText,
                currentPath,
                ref bestRank,
                ref bestPath);

            return bestPath;
        }

        private static void FindBestSourcePath(
            IEnumerable<SourceFilterViewModel> sources,
            string searchText,
            List<SourceFilterViewModel> currentPath,
            ref int bestRank,
            ref List<SourceFilterViewModel> bestPath)
        {
            foreach (var source in sources)
            {
                currentPath.Add(source);

                var rank = GetSourceMatchRank(source, searchText);
                if (rank < bestRank)
                {
                    bestRank = rank;
                    bestPath = new List<SourceFilterViewModel>(currentPath);
                }

                FindBestSourcePath(source.Children, searchText, currentPath, ref bestRank, ref bestPath);
                currentPath.RemoveAt(currentPath.Count - 1);
            }
        }

        private static int GetSourceMatchRank(SourceFilterViewModel source, string searchText)
        {
            var name = source.Name ?? string.Empty;
            var fullName = source.FullName ?? name;

            if (string.Equals(fullName, searchText, StringComparison.OrdinalIgnoreCase))
                return 0;
            if (string.Equals(name, searchText, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (fullName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                return 2;
            if (name.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                return 3;
            if (fullName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                return 4;
            if (name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                return 5;

            return int.MaxValue;
        }

        private void SelectLogSource(SourceFilterViewModel source)
        {
            if (_selectedLogSource != null)
            {
                _selectedLogSource.IsSelected = false;
            }

            _selectedLogSource = source;
            if (_selectedLogSource != null)
            {
                _selectedLogSource.IsSelected = true;
            }
        }


        // Fody
        // ReSharper disable once UnusedMember.Local
        private void OnSelectedLogMessageChanged()
        {
            if (SelectedLogMessage == null)
            {
                SelectLogSource(null);
                return;
            }

            SelectLogSource(ProfileFiltersViewModel.SourceFilterRootViewModel.Find(SelectedLogMessage.LoggerNames));
            //SelectedLogMessage.LastLoggerName
        }

        public string ToolTip
        {
            get
            {
                var receiverInfo = "LogFormat: " + Receiver.LogFormat;
                if (string.IsNullOrEmpty(Receiver.Description))
                {
                    string port = string.Empty;
                    string type = string.Empty;
                    string buffer = string.Empty;
                    string ipv6 = string.Empty;
                    switch (Receiver)
                    {
                        case TcpReceiver tcpReceiver:
                            port = tcpReceiver.Port.ToString();
                            type = "TCP";
                            buffer = tcpReceiver.BufferSize.ToString();
                            ipv6 = tcpReceiver.IpV6 ? "true" : "false";
                            break;
                        case UdpReceiver udpReceiver:
                            port = udpReceiver.Port.ToString();
                            type = "UDP";
                            buffer = udpReceiver.BufferSize.ToString();
                            ipv6 = udpReceiver.IpV6 ? "true" : "false";
                            break;
                    }
                    receiverInfo += Environment.NewLine + "Port: " + port;
                    receiverInfo += Environment.NewLine + "Type: " + type;
                    receiverInfo += Environment.NewLine + "Buffer: " + buffer;
                    receiverInfo += Environment.NewLine + "IpV6: " + ipv6;
                    return receiverInfo;
                }
                return Receiver.Description + Environment.NewLine + receiverInfo;
            }
        }

        public bool CanSyncWithSelectedItem => SelectedLogMessage != null;

        public CollectionViewSource CollectionViewSource
        {
            get
            {
                if (_collectionViewSource == null)
                {
                    _collectionViewSource = new CollectionViewSource();
                    _collectionViewSource.Filter += LogMessagesViewSourceFilter;
                    var binding = new Binding
                                  {
                                      IsAsync = true,
                                      Source = this,
                                      Mode = BindingMode.OneWay,
                                      NotifyOnSourceUpdated = true,
                                      Path = new PropertyPath("LogMessages")
                                  };
                    BindingOperations.SetBinding(_collectionViewSource, CollectionViewSource.SourceProperty, binding);
                }
                return _collectionViewSource;
            }
        }
        
        public async Task Rename()
        {
            var newName = await DialogService.Current.ShowInputDialog($"Rename: {DisplayName}", "Enter new name:");
            if (string.IsNullOrEmpty(newName))
            {
                return;
            }
            DisplayName = newName;
        }
        
        public void FindNext()
        {
            OnSearchTextChanged();
        }

        public void Clear()
        {
            LogMessages.Clear();
            ProfileFiltersViewModel.TimeFilter.ClearAvailableRange();
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
                Logger.Error(e, "Failed to copy log entries to clipboard");
                DialogService.Current.ShowErrorMessageBox(e);
            }
        }

        public string CurrentToText()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var row in CollectionViewSource.View.OfType<LogMessage>())
                {
                    sb.AppendLine(row.MessageSingleLine);
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to export log entries to text");
                DialogService.Current.ShowErrorMessageBox(e);
            }
            return null;
        }

        public void Initialize()
        {
            try
            {
                Logger.Info("Initializing receiver: {0} (Type={1})", Receiver.DisplayName, Receiver.GetType().Name);
                Receiver.NewMessage += OnNewMessage;
                Receiver.NewMessages += OnNewMessages;
                Receiver.Initialize();

                Update(true);
                Logger.Info("Receiver initialized successfully: {0}", Receiver.DisplayName);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to initialize receiver: {0}", Receiver.DisplayName);
                DialogService.Current.ShowErrorMessageBox(e);
                TryClose();
                throw;
            }
        }

        private void LogMessagesViewSourceFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;

            var resultRow = e.Item as LogMessage;
            if (_filterLogic.IsFiltered(resultRow))
                return;

            e.Accepted = true;
        }
        
        protected override void DoUpdate(bool full, bool scrollIntoSelected)
        {
            if (full)
            {
                _filterLogic.RebuildLeaves();
            }

            Execute.OnUIThread(() =>
            {
                var prevSelected = SelectedLogMessage;
                CollectionViewSource.View?.Refresh();
                if (CollectionViewSource.View != null && prevSelected != null)
                {
                    if (_filterLogic.IsFiltered(prevSelected))
                    {
                        var index = LogMessages.IndexOf(prevSelected);
                        if (index > 0)
                        {
                            foreach (var closestPrevMessage in LogMessages.Take(index).Reverse())
                            {
                                if (!_filterLogic.IsFiltered(closestPrevMessage))
                                {
                                    SelectedLogMessage = closestPrevMessage;
                                    break;
                                }
                            }
                        }
                    }
                }
                NotifyOfPropertyChange(nameof(ShownLogMessages));
                if (scrollIntoSelected)
                {
                    ScrollIntoSelected(true);
                }
            });
        }

        public void ScrollIntoSelected(bool forced = false)
        {
            if (forced || !AutoScroll)
            {
                ((dynamic)GetView())?.ScrollIntoSelected();
            }
        }

        public void CheckUncheck(bool value)
        {
            foreach (var sfvm in ProfileFiltersViewModel.SourceFilterRootViewModel.Children)
            {
                sfvm.IsChecked = value;
            }
        }

        public void CollapseExpand(bool value)
        {
            SetExpanded(ProfileFiltersViewModel.SourceFilterRootViewModel.Children, value);
        }

        private static void SetExpanded(IEnumerable<SourceFilterViewModel> sources, bool value)
        {
            foreach (var source in sources)
            {
                source.IsExpanded = value;
                SetExpanded(source.Children, value);
            }
        }

        public void ClearSearchText()
        {
            SearchSourceText = string.Empty;
        }

        #region OnNewMessages

        private void UpdateTimeRange(IEnumerable<LogMessage> messages)
        {
            DateTime? start = null;
            DateTime? end = null;
            foreach (var message in messages)
            {
                var time = message.TimeStamp;
                if (!start.HasValue || time < start.Value)
                    start = time;
                if (!end.HasValue || time > end.Value)
                    end = time;
            }
            if (start.HasValue)
                Execute.OnUIThread(() => ProfileFiltersViewModel.TimeFilter.IncludeRange(start.Value, end.Value));
        }
        private void OnNewMessages(object sender, IReadOnlyCollection<LogMessage> logMsgs)
        {
            lock (LogMessages)
            {
                UpdateTimeRange(logMsgs);
                LogMessages.AddRange(logMsgs);
                foreach (var logMessage in logMsgs)
                {
                    ProfileFiltersViewModel.SourceFilterRootViewModel.Find(logMessage.LoggerNames);
                }

                if (Receiver.IsInitialized)
                    Update(true);
            }
        }

        private void OnNewMessage(object sender, LogMessage logMsg)
        {
            lock (LogMessages)
            {
                UpdateTimeRange(new[] { logMsg });
                LogMessages.Add(logMsg);
                ProfileFiltersViewModel.SourceFilterRootViewModel.Find(logMsg.LoggerNames);
                
                if (Receiver.IsInitialized)
                    Update(true);
            }
        }

        #endregion
        
        public void Handle(RefreshEvent message)
        {
            if (message.IsFilters)
            {
                ProfileFiltersViewModel.UpdateFilters();
            }
            Update(message.IsFull, true);
        }

        public void Dispose()
        {
            Logger.Info("Disposing LogPane: {0}", Receiver?.DisplayName ?? "(null)");
            LogPaneServices.EventAggregator.Unsubscribe(this);

            _searchTextChangedSubscriber?.Dispose();
            _utcTimeChangedSubscriber?.Dispose();

            if (Receiver != null)
            {
                Receiver.NewMessage -= OnNewMessage;
                Receiver.NewMessages -= OnNewMessages;
                Receiver.Terminate();
            }

            LogMessages.CollectionChanged -= LogMessagesOnCollectionChanged;
        }
    }
}
