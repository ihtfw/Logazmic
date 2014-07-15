using System.Windows;

namespace Logazmic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Logazmic.Core.Reciever;
    using Logazmic.Settings;
    using Logazmic.Utils;
    using Logazmic.ViewModels;

    using MahApps.Metro;

    using Microsoft.Shell;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: ISingleInstanceApp
    {
        private const string guid = "83FD7350-9ED4-4F41-BA38-A40017DFD8A8";

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
            if (SingleInstance<App>.InitializeAsFirstInstance(guid))
                new Bootstrapper();
            else
                Shutdown();
            
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (args.Count > 1)
                MainWindowViewModel.Instance.LoadFile(args[1]);
            ActivateWindow(MainWindow);
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
}
