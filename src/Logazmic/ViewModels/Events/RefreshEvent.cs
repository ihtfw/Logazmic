namespace Logazmic.ViewModels.Events
{
    public class RefreshEvent
    {
        private RefreshEvent(bool isFull, bool isFilters)
        {
            IsFull = isFull;
            IsFilters = isFilters;
        }

        public bool IsFull { get; }

        public bool IsFilters { get; }

        public static RefreshEvent Partial { get; set; } = new RefreshEvent(false, false);
        public static RefreshEvent Full { get; set; } = new RefreshEvent(true, false);
        public static RefreshEvent Filters { get; set; } = new RefreshEvent(true, true);
    }
}