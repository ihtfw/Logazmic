using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Logazmic.Core.Log;
using NuGet;

namespace Logazmic.Core.Filters
{
    public class FilterLogic
    {
        private readonly FiltersProfile _filtersProfile;

        private readonly HashSet<string> _logSourceLeaves = new HashSet<string>();

        public FilterLogic(FiltersProfile filtersProfile)
        {
            _filtersProfile = filtersProfile;
        }

        public bool IsFiltered(LogMessage logMessage)
        {
            if (logMessage == null)
            {
                return true;
            }

            if (logMessage.LogLevel < _filtersProfile.MinLogLevel)
            {
                return true;
            }

            if (!_filtersProfile.LogLevels.First(l => l.LogLevel == logMessage.LogLevel).IsEnabled)
            {
                return true;
            }

            foreach (var messageFilter in _filtersProfile.MessageFilters.Where(mf => mf.IsEnabled))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(logMessage.Message, messageFilter.Message, CompareOptions.IgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(_filtersProfile.FilterText))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(logMessage.Message, _filtersProfile.FilterText, CompareOptions.IgnoreCase) < 0)
                {
                    return true;
                }
            }

            if (!_logSourceLeaves.Contains(logMessage.LoggerName))
            {
                return true;
            }
            return false;
        }

        public void RebuildLeaves()
        {
            _logSourceLeaves.Clear();
            _logSourceLeaves.AddRange(_filtersProfile.SourceFilterRoot.Leaves().Where(l => l.IsChecked).Select(c => c.FullName));
        }
    }
}