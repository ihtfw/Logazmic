using System;
using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.Utils;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class TimeFilterViewModel : PropertyChangedBase
    {
        private readonly LogPaneServices _logPaneServices;
        private readonly FiltersProfile _filtersProfile;
        private string _startTime;
        private string _endTime;
        private DateTime? _availableStart;
        private DateTime? _availableEnd;
        private bool _startEdited;
        private bool _endEdited;
        private bool _utcTime = true;
        private readonly TimeZoneInfo _localTimeZone;

        public TimeFilterViewModel(LogPaneServices logPaneServices, FiltersProfile filtersProfile, TimeZoneInfo localTimeZone = null)
        {
            _logPaneServices = logPaneServices;
            _filtersProfile = filtersProfile;
            _localTimeZone = localTimeZone ?? TimeZoneInfo.Local;
            UpdateFilters();
        }

        public string StartWatermark => "From: " + DateTimeInput.Format;
        public string EndWatermark => "To: " + DateTimeInput.Format;

        public string StartTime
        {
            get => _startTime;
            set
            {
                _startEdited = true;
                _startTime = DateTimeInput.ApplyMask(value);
                Apply();
            }
        }

        public string EndTime
        {
            get => _endTime;
            set
            {
                _endEdited = true;
                _endTime = DateTimeInput.ApplyMask(value);
                Apply();
            }
        }

        public string Error { get; private set; }
        public bool HasError => !string.IsNullOrEmpty(Error);
        public DateTime? StartBoundary => DateTimeInput.TryParse(StartTime, out var value) ? value : null;
        public DateTime? EndBoundary => DateTimeInput.TryParse(EndTime, out var value) ? value : null;

        public void UpdateFilters()
        {
            _startEdited = _filtersProfile.StartTime.HasValue;
            _endEdited = _filtersProfile.EndTime.HasValue;
            UpdateDisplayedRange();
        }

        public void SetUtcTime(bool utcTime)
        {
            if (_utcTime == utcTime)
                return;

            _utcTime = utcTime;
            UpdateDisplayedRange();
        }

        private void UpdateDisplayedRange()
        {
            _startTime = DateTimeInput.FormatValue(ToDisplayTime(_startEdited ? _filtersProfile.StartTime : _availableStart));
            _endTime = DateTimeInput.FormatValue(ToDisplayTime(_endEdited ? _filtersProfile.EndTime : _availableEnd));
            Error = null;
            NotifyOfPropertyChange(nameof(StartTime));
            NotifyOfPropertyChange(nameof(EndTime));
            NotifyOfPropertyChange(nameof(StartBoundary));
            NotifyOfPropertyChange(nameof(EndBoundary));
        }

        // Automatic bounds describe the loaded data, but do not exclude later arrivals.
        // Once edited (or cleared), a bound belongs to the user and stops following the log.
        public void IncludeRange(DateTime start, DateTime end)
        {
            _availableStart = !_availableStart.HasValue || start < _availableStart ? start : _availableStart;
            _availableEnd = !_availableEnd.HasValue || end > _availableEnd ? end : _availableEnd;
            UpdateAutomaticBounds();
        }

        public void ClearAvailableRange()
        {
            _availableStart = null;
            _availableEnd = null;
            UpdateAutomaticBounds();
        }

        private void UpdateAutomaticBounds()
        {
            if (!_startEdited)
            {
                _startTime = DateTimeInput.FormatValue(ToDisplayTime(_availableStart));
                NotifyOfPropertyChange(nameof(StartTime));
                NotifyOfPropertyChange(nameof(StartBoundary));
            }
            if (!_endEdited)
            {
                _endTime = DateTimeInput.FormatValue(ToDisplayTime(_availableEnd));
                NotifyOfPropertyChange(nameof(EndTime));
                NotifyOfPropertyChange(nameof(EndBoundary));
            }
        }

        private void Apply()
        {
            if (!DateTimeInput.TryParse(StartTime, out var startTime) || !DateTimeInput.TryParse(EndTime, out var endTime))
            {
                Error = "Use " + DateTimeInput.Format + " (optional fractional seconds). Previous range remains active.";
                return;
            }

            // An automatic endpoint must not prevent extending the range beyond the data loaded so far.
            if (_startEdited && _endEdited && startTime.HasValue && endTime.HasValue && startTime > endTime)
            {
                Error = "Start must not be after end. Previous range remains active.";
                return;
            }

            try
            {
                startTime = _startEdited ? ToStoredTime(startTime) : null;
                endTime = _endEdited ? ToStoredTime(endTime) : null;
            }
            catch (ArgumentException)
            {
                Error = "This local time does not exist due to a daylight saving time transition. Previous range remains active.";
                return;
            }

            Error = null;
            if (_filtersProfile.StartTime == startTime && _filtersProfile.EndTime == endTime)
                return;

            _filtersProfile.StartTime = startTime;
            _filtersProfile.EndTime = endTime;
            _logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Partial);
        }

        private DateTime? ToDisplayTime(DateTime? value)
        {
            if (!value.HasValue || _utcTime || value.Value.Kind == DateTimeKind.Local)
                return value;

            // The log UI treats Unspecified timestamps as UTC in ToLocalTime().
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc), _localTimeZone);
        }

        private DateTime? ToStoredTime(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            var kind = _availableStart?.Kind ?? _filtersProfile.StartTime?.Kind
                ?? _filtersProfile.EndTime?.Kind ?? DateTimeKind.Unspecified;
            if (_utcTime || kind == DateTimeKind.Local)
                return DateTime.SpecifyKind(value.Value, kind);

            var utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified), _localTimeZone);
            return DateTime.SpecifyKind(utc, kind);
        }
    }
}
