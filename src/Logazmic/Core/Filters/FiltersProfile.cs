using System;
using System.Collections.Generic;
using System.Linq;
using Logazmic.Core.Log;

namespace Logazmic.Core.Filters
{
    public class FiltersProfile 
    {
        private List<LogLevelFilter> _enabledLogLevels;
        private List<MessageFilter> _messageFilters;
        private SourceFilter _sourceFilterRoot;
        private string _name;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = "New profile";
                }
                return _name;
            }
            set => _name = value;
        }

        public LogLevel MinLogLevel { get; set; } = LogLevel.Trace;

        public List<LogLevelFilter> LogLevels
        {
            get { return _enabledLogLevels ?? (_enabledLogLevels = AvailibleLogLevels().Select(l => new LogLevelFilter(l)).ToList()); }
            set => _enabledLogLevels = value;
        }

        public List<MessageFilter> MessageFilters
        {
            get => _messageFilters ?? (_messageFilters = new List<MessageFilter>());
            set => _messageFilters = value;
        }

        public string FilterText { get; set; }

        public SourceFilter SourceFilterRoot
        {
            get => _sourceFilterRoot ?? (_sourceFilterRoot = new SourceFilter{ Name = "Root" });
            set => _sourceFilterRoot = value;
        }

        public static IEnumerable<LogLevel> AvailibleLogLevels()
        {
            return Enum.GetValues(typeof(LogLevel)).OfType<LogLevel>().Where(l => l != LogLevel.None);
        }
        
        public void Apply(FiltersProfile other)
        {
            if (other == null)
                return;

            Name = other.Name;
            MinLogLevel = other.MinLogLevel;
            FilterText = other.FilterText;

            MessageFilters.Clear();

            foreach (var messageFilter in other.MessageFilters)
            {
                MessageFilters.Add(new MessageFilter(messageFilter.Message)
                {
                    IsEnabled = messageFilter.IsEnabled
                });
            }

            foreach (var logLevel in LogLevels)
            {
                logLevel.IsEnabled =
                    other.LogLevels.FirstOrDefault(ll => ll.LogLevel == logLevel.LogLevel)?.IsEnabled ?? true;
            }

            SourceFilterRoot = other.SourceFilterRoot.Clone();
        }

    }
}