using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logazmic.ViewModels
{
    using Caliburn.Micro;

    using Logazmic.Core.Log;
    using Logazmic.Services;

    public class LogLevelViewModel : PropertyChangedBase
    {
        private bool isEnabled;

        public LogLevel LogLevel { get; private set; }

        public LogLevelViewModel(LogLevel logLevel)
        {
            LogLevel = logLevel;
            IsEnabled = true;
        }

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