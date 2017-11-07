using System;
using System.Collections.Generic;
using System.Linq;
using Logazmic.Core.Log;
using Logazmic.Settings;

namespace Logazmic.Core.Filters
{
    public class FiltersProfile 
    {
        private List<LogLevelFilter> enabledLogLevels;
        private List<MessageFilter> messageFilters;
        private SourceFilter sourceFilterRoot;
        private string name;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = "New profile";
                }
                return name;
            }
            set { name = value; }
        }

        public LogLevel MinLogLevel { get; set; } = LogLevel.Trace;

        public List<LogLevelFilter> LogLevels
        {
            get { return enabledLogLevels ?? (enabledLogLevels = AvailibleLogLevels().Select(l => new LogLevelFilter(l)).ToList()); }
            set { enabledLogLevels = value; }
        }

        public List<MessageFilter> MessageFilters
        {
            get { return messageFilters ?? (messageFilters = new List<MessageFilter>()); }
            set { messageFilters = value; }
        }

        public string FilterText { get; set; }

        public SourceFilter SourceFilterRoot
        {
            get { return sourceFilterRoot ?? (sourceFilterRoot = new SourceFilter{ Name = "Root" }); }
            set { sourceFilterRoot = value; }
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