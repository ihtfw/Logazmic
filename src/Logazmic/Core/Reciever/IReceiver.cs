namespace Logazmic.Core.Reciever
{
    using System.ComponentModel;

    using Logazmic.Core.Log;

    public interface IReceiver
    {
        void Initialize();
        void Terminate();

        void Attach(ILogMessageNotifiable notifiable);
        void Detach();

        [Browsable(false)]
        string DisplayName { get; set; }

        bool IsInitilized { get; set; }
    }
}