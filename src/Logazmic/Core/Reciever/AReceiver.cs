namespace Logazmic.Core.Reciever
{
    using System;

    using Logazmic.Core.Log;

    public abstract class AReceiver : IDisposable
    {
        private bool isInitilized;

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

        public abstract void Terminate();

        protected abstract void DoInitilize();

        #region Events

        public event EventHandler<LogMessage> NewMessage;

        public event EventHandler<LogMessage[]> NewMessages;

        #region Invocators

        protected virtual void OnNewMessage(LogMessage e)
        {
            var handler = NewMessage;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNewMessages(LogMessage[] e)
        {
            var handler = NewMessages;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion


        #endregion

        public void Dispose()
        {
            Terminate();
        }
    }
}