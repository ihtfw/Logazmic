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
        private readonly LogPaneServices logPaneServices;

        private bool isEnabled;

        public LogLevel LogLevel { get; private set; }

        public LogLevelViewModel(LogPaneServices logPaneServices, LogLevel logLevel)
        {
            this.logPaneServices = logPaneServices;
            LogLevel = logLevel;
            IsEnabled = true;
        }

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