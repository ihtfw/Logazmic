using System.Linq;
using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class ProfileFiltersViewModel : PropertyChangedBase
    {
        private readonly LogPaneServices _logPaneServices;
        private LogLevelFilterViewModel _minLogLevel;

        public FiltersProfile FiltersProfile { get; } 

        public ProfileFiltersViewModel(FiltersProfile filtersProfile, LogPaneServices logPaneServices)
        {
            FiltersProfile = filtersProfile;
            _logPaneServices = logPaneServices;

            LogLevels = new BindableCollection<LogLevelFilterViewModel>(
                FiltersProfile.LogLevels.Select(ll => new LogLevelFilterViewModel(logPaneServices, ll)));

            MinLogLevel = LogLevels.First();

            SourceFilterRootViewModel = new SourceFilterViewModel(filtersProfile.SourceFilterRoot, logPaneServices);
        }


        public string SearchText { get; set; }

        public SourceFilterViewModel SourceFilterRootViewModel { get; }

        public string FilterText
        {
            get => FiltersProfile.FilterText;
            set
            {
                FiltersProfile.FilterText = value;
                
                _logPaneServices.EventAggregator.PublishOnCurrentThreadAsync(RefreshEvent.Partial);
            }
        }
        public BindableCollection<LogLevelFilterViewModel> LogLevels { get; }

        public LogLevelFilterViewModel MinLogLevel
        {
            get => _minLogLevel;
            set
            {
                _minLogLevel = value;
                if (_minLogLevel != null)
                {
                    FiltersProfile.MinLogLevel = _minLogLevel.LogLevel;
                    _logPaneServices.EventAggregator.PublishOnCurrentThreadAsync(RefreshEvent.Partial);
                }
            }
        }

        public BindableCollection<MessageFilterViewModel> MessageFilters { get; } = new();

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

            var filter = new MessageFilter(messageFilter);
            FiltersProfile.MessageFilters.Add(filter);
            var messageFilterViewModel = new MessageFilterViewModel(_logPaneServices, filter);
            MessageFilters.Add(messageFilterViewModel);

            _logPaneServices.EventAggregator.PublishOnCurrentThreadAsync(RefreshEvent.Partial);
        }

        public void RemoveMessageFilter(MessageFilterViewModel messageFilterViewModel)
        {
            if (messageFilterViewModel == null)
            {
                return;
            }

            FiltersProfile.MessageFilters.Remove(messageFilterViewModel.MessageFilter);
            MessageFilters.Remove(messageFilterViewModel);

            _logPaneServices.EventAggregator.PublishOnCurrentThreadAsync(RefreshEvent.Partial);
        }
        
        public void UpdateFilters()
        {
            SearchText = null;

            MessageFilters.Clear();
            MessageFilters.AddRange(FiltersProfile.MessageFilters.Select(mf => new MessageFilterViewModel(_logPaneServices, mf)));

            MinLogLevel = null;
            LogLevels.Clear();
            LogLevels.AddRange(FiltersProfile.LogLevels.Select(ll => new LogLevelFilterViewModel(_logPaneServices, ll)));
            MinLogLevel = LogLevels.FirstOrDefault(ll => ll.LogLevel == FiltersProfile.MinLogLevel) ?? LogLevels.First();

            SourceFilterRootViewModel.Rebuild(FiltersProfile.SourceFilterRoot);
            NotifyOfPropertyChange(nameof(FilterText));
        }
    }
}