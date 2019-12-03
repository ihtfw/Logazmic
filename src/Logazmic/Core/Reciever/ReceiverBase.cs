using Logazmic.Core.Readers;

namespace Logazmic.Core.Reciever
{
    using System;

    using Log;

    using Newtonsoft.Json;

    public abstract class ReceiverBase : IDisposable
    {
        private bool _isInitilized;

        public string DisplayName { get; set; }

        [JsonIgnore]
        public virtual string Description { get { return null; } }
        
        /// <summary>
        /// Must be set externally!
        /// </summary>
        [JsonIgnore]
        public ILogReaderFactory LogReaderFactory { get; set; }

        public string LogFormat { get; set; }

        public void Dispose()
        {
            Terminate();
        }

        public void Initialize()
        {
            if (_isInitilized)
            {
                return;
            }
            DoInitilize();
            _isInitilized = true;
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
    }
}