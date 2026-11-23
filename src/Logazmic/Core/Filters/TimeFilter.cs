using System;

namespace Logazmic.Core.Filters
{
    public readonly struct TimeFilter
    {
        public TimeFilter(DateTime? startTime, DateTime? endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public DateTime? StartTime { get; }
        public DateTime? EndTime { get; }

        public bool Contains(DateTime timeStamp)
        {
            return (!StartTime.HasValue || timeStamp >= StartTime.Value)
                && (!EndTime.HasValue || timeStamp <= EndTime.Value);
        }
    }
}
