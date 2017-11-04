using System;
using System.Collections.Generic;
using System.Linq;
using Logazmic.Core.Log;

namespace Logazmic.Core.Filters
{
    public class FiltersProfile
    {
        private List<LogLevelFilter> enabledLogLevels;
        private List<MessageFilter> messageFilters;
        private SourceFilter sourceFilterRoot;

        public string Name { get; set; } = "New profile";

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

        public void CopyFrom(FiltersProfile filtersProfile)
        {
            if (filtersProfile == null)
                return;


        }
    }
}