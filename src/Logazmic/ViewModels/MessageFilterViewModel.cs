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
        private bool isEnabled;

        public MessageFilterViewModel(string messageFilter)
        {
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
                Messaging.Publish(new RefreshEvent());
            }
        }
    }
}