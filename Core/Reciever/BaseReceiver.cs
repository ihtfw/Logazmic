namespace Logazmic.Core.Reciever
{
    using System;
    using System.ComponentModel;

    using Logazmic.Core.Log;

    [Serializable]
    public abstract class BaseReceiver : MarshalByRefObject, IReceiver
    {
        [NonSerialized]
        protected ILogMessageNotifiable Notifiable;

        [NonSerialized]
        private string _displayName;


        #region IReceiver Members

        public abstract string SampleClientConfig { get; }

        [Browsable(false)]
        public string DisplayName
        {
            get { return _displayName; }
            protected set { _displayName = value; }
        }

        public abstract void Initialize();
        public abstract void Terminate();

        public virtual void Attach(ILogMessageNotifiable notifiable)
        {
            Notifiable = notifiable;
        }

        public virtual void Detach()
        {
            Notifiable = null;
        }

        #endregion
    }
}
