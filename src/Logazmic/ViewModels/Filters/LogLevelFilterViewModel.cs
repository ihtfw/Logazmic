using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.Core.Log;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class LogLevelFilterViewModel : PropertyChangedBase
    {
        private readonly LogPaneServices logPaneServices;
        private readonly LogLevelFilter logLevelFilter;

        public LogLevel LogLevel => logLevelFilter.LogLevel;

        public LogLevelFilterViewModel(LogPaneServices logPaneServices, LogLevelFilter logLevelFilter)
        {
            this.logPaneServices = logPaneServices;
            this.logLevelFilter = logLevelFilter;
        }

        public bool IsEnabled
        {
            get { return logLevelFilter.IsEnabled; }
            set
            {
                logLevelFilter.IsEnabled = value;
                logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Partial);
            }
        }
    }
}