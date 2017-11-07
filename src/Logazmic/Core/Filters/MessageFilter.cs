namespace Logazmic.Core.Filters
{
    public class MessageFilter
    {
        public MessageFilter(string message)
        {
            Message = message;
        }

        public string Message { get;  }
        public bool IsEnabled { get; set; } = true;

        public override string ToString()
        {
            return $"{Message} - {IsEnabled}";
        }
    }
}