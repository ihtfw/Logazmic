namespace Logazmic.ViewModels
{
    using Caliburn.Micro;

    public class LogPaneServices
    {
        public IEventAggregator EventAggregator { get; set; } = new EventAggregator();
    }
    
}