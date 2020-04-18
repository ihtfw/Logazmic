using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class MessageFilterViewModel : PropertyChangedBase
    {
        private readonly LogPaneServices _logPaneServices;
        
        public  MessageFilter MessageFilter { get; }

        public MessageFilterViewModel(LogPaneServices logPaneServices, MessageFilter messageFilter)
        {
            _logPaneServices = logPaneServices;
            MessageFilter = messageFilter;
        }

        public string Filter => MessageFilter.Message;

        public bool IsEnabled
        {
            get => MessageFilter.IsEnabled;
            set
            {
                MessageFilter.IsEnabled = value;
                _logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Partial);
            }
        }
    }
}