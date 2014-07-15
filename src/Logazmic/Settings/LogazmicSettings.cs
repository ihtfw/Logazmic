namespace Logazmic.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Controls;

    using Logazmic.Behaviours;
    using Logazmic.Core.Reciever;

    public class LogazmicSettings : JsonSettingsBase
    {
        #region Singleton

        private static readonly Lazy<LogazmicSettings> instance = new Lazy<LogazmicSettings>(() => Load<LogazmicSettings>(path));

        public static LogazmicSettings Instance { get { return instance.Value; } }

        #endregion

        private static readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Logazmic\settings.json");

        public ObservableCollection<ReceiverBase> Receivers { get; set; }

        [DefaultValue(12)]
        public int GridFontSize { get; set; }

        [DefaultValue(18)]
        public int DescriptionFontSize { get; set; }

        [DefaultValue(false)]
        public bool UseDarkTheme { get; set; }

        [DefaultValue(DataGridGridLinesVisibility.None)]
        public DataGridGridLinesVisibility GridLinesVisibility { get; set; }

        protected override void SetDefaults()
        {
            base.SetDefaults();
            Receivers = new ObservableCollection<ReceiverBase>();
        }

        public override void Save()
        {
            Save(path);
        }
    }
}