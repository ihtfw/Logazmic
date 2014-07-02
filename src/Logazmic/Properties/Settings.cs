namespace Logazmic.Properties
{
    using System.Collections.Generic;

    using Logazmic.Core.Reciever;

    internal partial class Settings
    {
        public Settings()
        {
            PropertyChanged += (sender, args) => Save();
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("18")]
        public List<IReceiver> Receivers
        {
            get
            {
                return ((List<IReceiver>)(this["Receivers"]));
            }
            set
            {
                this["Receivers"] = value;
            }
        }
    }
}