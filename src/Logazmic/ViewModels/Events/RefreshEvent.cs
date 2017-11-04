namespace Logazmic.ViewModels.Events
{
    public class RefreshEvent
    {
        private RefreshEvent(bool isFull)
        {
            IsFull = isFull;
        }

        public bool IsFull { get; }

        public static RefreshEvent Partial { get; set; } = new RefreshEvent(false);
        public static RefreshEvent Full { get; set; } = new RefreshEvent(true);
    }
}