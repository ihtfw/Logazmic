using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.Core.Log;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class LogLevelFilterViewModel : PropertyChangedBase
    {
        private readonly LogPaneServices _logPaneServices;
        private readonly LogLevelFilter _logLevelFilter;

        public LogLevel LogLevel => _logLevelFilter.LogLevel;

        public LogLevelFilterViewModel(LogPaneServices logPaneServices, LogLevelFilter logLevelFilter)
        {
            _logPaneServices = logPaneServices;
            _logLevelFilter = logLevelFilter;
        }

        public bool IsEnabled
        {
            get => _logLevelFilter.IsEnabled;
            set
            {
                _logLevelFilter.IsEnabled = value;
                _logPaneServices.EventAggregator.PublishOnCurrentThreadAsync(RefreshEvent.Partial);
            }
        }
    }
}