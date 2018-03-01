using Logazmic.Core.Filters;

namespace Logazmic.Settings
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Controls;

    using Core.Reciever;

    
    public class LogazmicSettings : JsonSettingsBase
    {
        #region Singleton

        private static readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Logazmic\settings.json");

        private static readonly Lazy<LogazmicSettings> instance = new Lazy<LogazmicSettings>(() => Load<LogazmicSettings>(path));

        public static LogazmicSettings Instance { get { return instance.Value; } }
        public override void Save()
        {
            Save(path);
        }

        #endregion
        
        private FiltersProfiles filtersProfiles;
        private ObservableCollection<ReceiverBase> receivers;

        public ObservableCollection<ReceiverBase> Receivers
        {
            get { return receivers ?? (receivers = new ObservableCollection<ReceiverBase>()); }
            set { receivers = value; }
        }

        [DefaultValue(12)]
        public int GridFontSize { get; set; }

        [DefaultValue(18)]
        public int DescriptionFontSize { get; set; }

        [DefaultValue(false)]
        public bool UseDarkTheme { get; set; }

        [DefaultValue(true)]
        public bool ShowStatusBar { get; set; }

        [DefaultValue(DataGridGridLinesVisibility.None)]
        public DataGridGridLinesVisibility GridLinesVisibility { get; set; }

        public bool? AutoUpdate { get; set; }

        public bool EnableSelfLogging { get; set; }

        public int SelfLoggingPort { get; set; } = 6789;
    }
}