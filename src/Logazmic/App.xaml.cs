using System.Windows;

namespace Logazmic
{
    using System;
    using System.Linq;


    using Logazmic.Settings;
    using Logazmic.Utils;

    using MahApps.Metro;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
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
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themeFile, UriKind.Relative) });

            var appTheme = ThemeManager.GetAppTheme(LogazmicSettings.Instance.UseDarkTheme ? "BaseDark" : "BaseLight");
            var accent = ThemeManager.GetAccent("Cyan");
            ThemeManager.ChangeAppStyle(this, accent, appTheme);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ChangeTheme();
        }
    }
}
