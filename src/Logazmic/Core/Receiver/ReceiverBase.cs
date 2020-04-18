using System;
using System.Collections.Generic;
using Logazmic.Core.Log;
using Logazmic.Core.Readers;
using Newtonsoft.Json;

namespace Logazmic.Core.Receiver
{
    public abstract class ReceiverBase : IDisposable
    {
        public bool IsInitialized { get; private set; }

        public string DisplayName { get; set; }

        [JsonIgnore]
        public virtual string Description => null;

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
            if (IsInitialized)
            {
                return;
            }
            DoInitialize();
            IsInitialized = true;

            Initialized?.Invoke(this, EventArgs.Empty);
        }

        public abstract void Terminate();

        protected abstract void DoInitialize();

        #region Events

        public event EventHandler Initialized;

        public event EventHandler<LogMessage> NewMessage;

        public event EventHandler<IReadOnlyCollection<LogMessage>> NewMessages;

        #region Invocators

        protected virtual void OnNewMessage(LogMessage e)
        {
            NewMessage?.Invoke(this, e);
        }

        protected virtual void OnNewMessages(IReadOnlyCollection<LogMessage> e)
        {
            NewMessages?.Invoke(this, e);
        }

        #endregion

        #endregion
    }
}