using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Logazmic.Core.Log;

namespace Logazmic.Core.Filters
{
    public class FilterLogic
    {
        private readonly FiltersProfile filtersProfile;

        private List<string> logSourceLeaves = new List<string>();

        public FilterLogic(FiltersProfile filtersProfile)
        {
            this.filtersProfile = filtersProfile;
        }

        public bool IsFiltered(LogMessage logMessage)
        {
            if (logMessage == null)
            {
                return true;
            }

            if (logMessage.LogLevel < filtersProfile.MinLogLevel)
            {
                return true;
            }

            if (!filtersProfile.LogLevels.First(l => l.LogLevel == logMessage.LogLevel).IsEnabled)
            {
                return true;
            }

            foreach (var messageFilter in filtersProfile.MessageFilters.Where(mf => mf.IsEnabled))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(logMessage.Message, messageFilter.Message, CompareOptions.IgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(filtersProfile.FilterText))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(logMessage.Message, filtersProfile.FilterText, CompareOptions.IgnoreCase) < 0)
                {
                    return true;
                }
            }

            if (logSourceLeaves.All(l => l != logMessage.LoggerName))
            {
                return true;
            }
            return false;
        }

        public void RebuildLeaves()
        {
            logSourceLeaves = filtersProfile.SourceFilterRoot.Leaves().Where(l => l.IsChecked).Select(c => c.FullName).Distinct().ToList();
        }
    }
}