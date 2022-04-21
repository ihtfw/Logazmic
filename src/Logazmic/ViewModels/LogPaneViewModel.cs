using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Logazmic.Core.Filters;
using Logazmic.Core.Receiver;
using Logazmic.Utils;
using Logazmic.ViewModels.Events;
using Logazmic.ViewModels.Filters;

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
        private SourceFilterViewModel _selectedLogSource;
        private CollectionViewSource _collectionViewSource;

        private readonly IDisposable _searchTextChangedSubscriber;
        private readonly FilterLogic _filterLogic;
        public LogPaneViewModel([NotNull] ReceiverBase receiver)
        {
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            var filtersProfile = new FiltersProfile();
            LogMessages.CollectionChanged += LogMessagesOnCollectionChanged;

            ProfileFiltersViewModel = new ProfileFiltersViewModel(filtersProfile, LogPaneServices);
            _filterLogic = new FilterLogic(filtersProfile);
            ProfilesFiltersViewModel = new ProfilesFiltersViewModel(filtersProfile, LogPaneServices);
            _searchTextChangedSubscriber = ProfileFiltersViewModel.SubscribeToPropertyChanged(vm => vm.SearchText, OnSearchTextChanged);

            ProfilesFiltersViewModel.ActivateWith(this);

            LogPaneServices.EventAggregator.Subscribe(this);
        }


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

        // Fody
        // ReSharper disable once UnusedMember.Local
        private void OnSelectedLogMessageChanged()
        {
            if (SelectedLogMessage == null)
            {
                if (_selectedLogSource != null)
                {
                    _selectedLogSource.IsSelected = false;
                }
                _selectedLogSource = null;
                return;
            }

            _selectedLogSource = ProfileFiltersViewModel.SourceFilterRootViewModel.Find(SelectedLogMessage.LoggerNames);
            if (_selectedLogSource != null)
            {
                _selectedLogSource.IsSelected = true;
            }
            //SelectedLogMessage.LastLoggerName
        }

        public string ToolTip
        {
            get
            {
                var format = "LogFormat: " + Receiver.LogFormat;
                if (string.IsNullOrEmpty(Receiver.Description))
                {
                    return format;
                }
                return Receiver.Description + Environment.NewLine + format;
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
            var newName = await DialogService.Current.ShowInputDialog("Rename", "Enter new name:");
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
            if (_filterLogic.IsFiltered(resultRow))
                return;

            e.Accepted = true;
        }
        
        protected override void DoUpdate(bool full)
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
            });

           // ScrollIntoSelected();
        }

        public void ScrollIntoSelected(bool forced = false)
        {
            if (forced || !AutoScroll)
            {
                ((dynamic)GetView())?.ScrollIntoSelected();
            }
        }

        #region OnNewMessages

        private void OnNewMessages(object sender, IReadOnlyCollection<LogMessage> logMsgs)
        {
            lock (LogMessages)
            {
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
            Update(message.IsFull);
        }

        public void Dispose()
        {
            LogPaneServices.EventAggregator.Unsubscribe(this);

            _searchTextChangedSubscriber?.Dispose();

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