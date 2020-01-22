using Logazmic.Core.Filters;
using Logazmic.Core.Readers;

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

        public static LogazmicSettings Instance => instance.Value;

        public override void Save()
        {
            Save(path);
        }

        #endregion
        
        private ObservableCollection<ReceiverBase> _receivers;

        public ObservableCollection<ReceiverBase> Receivers
        {
            get => _receivers ?? (_receivers = new ObservableCollection<ReceiverBase>());
            set => _receivers = value;
        }

        public int GridFontSize { get; set; } = 12;

        public int DescriptionFontSize { get; set; } = 18;

        public bool UseDarkTheme { get; set; }

        public bool ShowStatusBar { get; set; } = true;

        public bool UtcTime { get; set; } 

        public bool Use24HourFormat { get; set; } = true;

        public DataGridGridLinesVisibility GridLinesVisibility { get; set; } = DataGridGridLinesVisibility.None;

        public bool? AutoUpdate { get; set; }

        public bool EnableSelfLogging { get; set; }

        public int SelfLoggingPort { get; set; } = 6789;

        public string LogFormat { get; set; } = LogFormats.Log4j;
    }
}