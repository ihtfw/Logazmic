namespace Logazmic.Core.Reciever
{
    using Logazmic.Core.Log;

    public abstract class ReceiverBase : IReceiver
    {
        protected ILogMessageNotifiable Notifiable;

        #region IReceiver Members

        public string DisplayName { get; set; }

        public void Initialize()
        {
            if (isInitilized)
            {
                return;
            }
            DoInitilize();
            isInitilized = true;
        }

        protected bool isInitilized;

        public abstract void Terminate();

        public virtual void Attach(ILogMessageNotifiable notifiable)
        {
            Notifiable = notifiable;
        }

        public virtual void Detach()
        {
            Notifiable = null;
        }

        protected abstract void DoInitilize();

        #endregion
    }
}