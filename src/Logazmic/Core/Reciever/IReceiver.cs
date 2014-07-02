namespace Logazmic.Core.Reciever
{
    using Logazmic.Core.Log;

    public interface IReceiver
    {
        string SampleClientConfig { get; }
        string DisplayName { get; }

        void Initialize();
        void Terminate();

        void Attach(ILogMessageNotifiable notifiable);
        void Detach();
    }
}