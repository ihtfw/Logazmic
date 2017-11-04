using System.Linq;
using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class ProfileFiltersViewModel : PropertyChangedBase
    {
        private readonly LogPaneServices logPaneServices;
        private LogLevelFilterViewModel minLogLevel;

        public FiltersProfile FiltersProfile { get; } 

        public ProfileFiltersViewModel(FiltersProfile filtersProfile, LogPaneServices logPaneServices)
        {
            FiltersProfile = filtersProfile;
            this.logPaneServices = logPaneServices;

            LogLevels = new BindableCollection<LogLevelFilterViewModel>(
                FiltersProfile.LogLevels.Select(ll => new LogLevelFilterViewModel(logPaneServices, ll)));

            MinLogLevel = LogLevels.First();

            SourceFilterRootViewModel = new SourceFilterViewModel(filtersProfile.SourceFilterRoot, logPaneServices);
        }

        private void SetFiltersProfile(FiltersProfile filtersProfile)
        {
            MinLogLevel = LogLevels.FirstOrDefault(ll => ll.LogLevel == filtersProfile.MinLogLevel) ?? LogLevels.First();
        }

        public string SearchText { get; set; }

        public SourceFilterViewModel SourceFilterRootViewModel { get; }

        public string FilterText
        {
            get { return FiltersProfile.FilterText; }
            set
            {
                FiltersProfile.FilterText = value;
                
                logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Partial);
            }
        }
        public BindableCollection<LogLevelFilterViewModel> LogLevels { get; }

        public LogLevelFilterViewModel MinLogLevel
        {
            get { return minLogLevel; }
            set
            {
                minLogLevel = value;
                FiltersProfile.MinLogLevel = minLogLevel.LogLevel;

                logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Partial);
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

            var filter = new MessageFilter(messageFilter);
            FiltersProfile.MessageFilters.Add(filter);
            var messageFilterViewModel = new MessageFilterViewModel(logPaneServices, filter);
            MessageFilters.Add(messageFilterViewModel);

            logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Partial);
        }

        public void RemoveMessageFilter(MessageFilterViewModel messageFilterViewModel)
        {
            if (messageFilterViewModel == null)
            {
                return;
            }

            FiltersProfile.MessageFilters.Remove(messageFilterViewModel.MessageFilter);
            MessageFilters.Remove(messageFilterViewModel);

            logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Partial);
        }
    }
}