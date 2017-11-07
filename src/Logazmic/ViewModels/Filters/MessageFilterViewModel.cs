using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class MessageFilterViewModel : PropertyChangedBase
    {
        private readonly LogPaneServices logPaneServices;
        
        public  MessageFilter MessageFilter { get; }

        public MessageFilterViewModel(LogPaneServices logPaneServices, MessageFilter messageFilter)
        {
            this.logPaneServices = logPaneServices;
            MessageFilter = messageFilter;
        }

        public string Filter
        {
            get { return MessageFilter.Message; }
        }

        public bool IsEnabled
        {
            get { return MessageFilter.IsEnabled; }
            set
            {
                MessageFilter.IsEnabled = value;
                logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Partial);
            }
        }
    }
}