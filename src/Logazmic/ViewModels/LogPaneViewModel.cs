using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Logazmic.Core.Filters;
using Logazmic.Utils;
using Logazmic.ViewModels.Events;
using Logazmic.ViewModels.Filters;

namespace Logazmic.ViewModels
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;

    using Caliburn.Micro;
    
    using Core.Log;
    using Core.Reciever;
    using Services;
    using Settings;

    public class LogPaneViewModel : UpdatableScreen, IHandle<RefreshEvent>, IDisposable
    {
        private SourceFilterViewModel selectedLogSource;
        private CollectionViewSource collectionViewSource;

        private readonly IDisposable searchTextChangedSubscriber;
        private readonly FilterLogic filterLogic;
        public LogPaneViewModel([NotNull] ReceiverBase receiver)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            Receiver = receiver;
            var filtersProfile = new FiltersProfile();
            LogMessages.CollectionChanged += LogMessagesOnCollectionChanged;

            ProfileFiltersViewModel = new ProfileFiltersViewModel(filtersProfile, LogPaneServices);
            filterLogic = new FilterLogic(filtersProfile);
            ProfilesFiltersViewModel = new ProfilesFiltersViewModel(filtersProfile, LogPaneServices);
            searchTextChangedSubscriber = ProfileFiltersViewModel.SubscribeToPropertyChanged(vm => vm.SearchText, OnSearchTextChanged);

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

        public int ShownLogMessages => collectionViewSource?.View?.Cast<LogMessage>().Count() ?? 0;

        public override string DisplayName { get { return Receiver?.DisplayName; } set { Receiver.DisplayName = value; } }
        
        public ReceiverBase Receiver { get; }

        public BindableCollection<LogMessage> LogMessages { get; } =new BindableCollection<LogMessage>();

        public LogMessage SelectedLogMessage { get; set; }

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

            selectedLogSource = ProfileFiltersViewModel.SourceFilterRootViewModel.Find(SelectedLogMessage.LoggerNames);
            if (selectedLogSource != null)
            {
                selectedLogSource.IsSelected = true;
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
            if (filterLogic.IsFiltered(resultRow))
                return;

            e.Accepted = true;
        }
        
        protected override void DoUpdate(bool full)
        {
            if (full)
            {
                filterLogic.RebuildLeaves();
            }

            Execute.OnUIThread(() =>
            {
                var prevSelected = SelectedLogMessage;
                CollectionViewSource.View?.Refresh();
                if (CollectionViewSource.View != null && prevSelected != null)
                {
                    if (filterLogic.IsFiltered(prevSelected))
                    {
                        var index = LogMessages.IndexOf(prevSelected);
                        if (index > 0)
                        {
                            foreach (var closestPrevMessage in LogMessages.Take(index).Reverse())
                            {
                                if (!filterLogic.IsFiltered(closestPrevMessage))
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

            searchTextChangedSubscriber?.Dispose();

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