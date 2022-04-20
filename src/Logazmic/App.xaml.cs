using System.Windows;
using Logazmic.Converters;
using Microsoft.Shell;

namespace Logazmic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Settings;
    using Utils;
    using ViewModels;

    using MahApps.Metro;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: ISingleInstanceApp
    {
        private const string Guid = "83FD7350-9ED4-4F41-BA38-A40017DFD8A8";

        public App()
        {
            LogazmicSettings.Instance.SubscribeToPropertyChanged(settings => settings.UseDarkTheme, ChangeTheme);
        }

        private void ChangeTheme()
        {
            var oldTheme = Resources.MergedDictionaries.SingleOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme"));
            if(oldTheme != null)
                Resources.MergedDictionaries.Remove(oldTheme);

            var themeFile = LogazmicSettings.Instance.UseDarkTheme ? "Styles/DarkTheme.xaml" : "Styles/LightTheme.xaml";
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) });

            var appTheme = ThemeManager.GetAppTheme(LogazmicSettings.Instance.UseDarkTheme ? "BaseDark" : "BaseLight");
            var accent = ThemeManager.GetAccent("Cyan");
            ThemeManager.ChangeAppStyle(this, accent, appTheme);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetDateTimeToStringConverterOptions();
            ChangeTheme();

            if (LogazmicSettings.Instance.SingleWindowMode)
            {
                if (SingleInstance<App>.InitializeAsFirstInstance(Guid))
                    // ReSharper disable once ObjectCreationAsStatement
                    new Bootstrapper();
                else
                    Shutdown();
            }
            else
            {
                new Bootstrapper();
            }
        }

        private void SetDateTimeToStringConverterOptions()
        {
            var dateTimeToStringConverter = (DateTimeToStringConverter) Resources["DateTimeToStringConverter"];
            dateTimeToStringConverter.Options = new DateTimeToStringConverterOptions(LogazmicSettings.Instance);
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if(MainWindow != null)
                ActivateWindow(MainWindow);
            if (args.Count > 1)
                MainWindowViewModel.Instance.LoadFile(args[1]);
            return true;
        }

        private void ActivateWindow(Window window)
        {
            if (!window.IsVisible)
            {
                window.Show();
            }

            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            window.Activate();
            window.Topmost = true;  
            window.Topmost = false; 
            window.Focus();      
        }
    }

    internal class DateTimeToStringConverterOptions : IDateTimeToStringConverterOptions
    {
        private readonly LogazmicSettings _logazmicSettings;

        public DateTimeToStringConverterOptions(LogazmicSettings logazmicSettings)
        {
            _logazmicSettings = logazmicSettings;
        }

        public bool UtcTime => _logazmicSettings.UtcTime;

        public bool Use24HourFormat => _logazmicSettings.Use24HourFormat;

        public bool DisplayingMilliseconds => _logazmicSettings.DisplayingMilliseconds;
    }
}
