using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logazmic.ViewModels
{
    using Caliburn.Micro;

    using Logazmic.Services;

    public class MessageFilterViewModel : PropertyChangedBase
    {
        private readonly LogPaneServices logPaneServices;

        private bool isEnabled;

        public MessageFilterViewModel(LogPaneServices logPaneServices, string messageFilter)
        {
            this.logPaneServices = logPaneServices;
            Filter = messageFilter;
            IsEnabled = true;
        }

        public string Filter { get; set; }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                logPaneServices.EventAggregator.PublishOnCurrentThread(new RefreshEvent());
            }
        }
    }
}