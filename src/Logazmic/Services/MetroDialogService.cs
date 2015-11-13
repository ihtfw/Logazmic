namespace Logazmic.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using Caliburn.Micro;

    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;

    using Microsoft.Win32;

    class MetroDialogService : DialogService
    {
        private MetroWindow GetActiveWindow()
        {
            MetroWindow window = null;

            Execute.OnUIThread(() =>
            {
                window = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault(w => w.IsActive);
                if (window == null)
                    window = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
            });
            
            return window;
        }

        public override void ShowMessageBox(string title, string message)
        {
            Execute.OnUIThread(() => GetActiveWindow().ShowMessageAsync(title, message));
        }

      
        public override async Task<bool?> ShowQuestionMessageBox(string title, string message)
        {
            MessageDialogResult result = default(MessageDialogResult);
            var metroDialogSettings = new MetroDialogSettings() { AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
            await Execute.OnUIThreadAsync(async () =>
            {
                result = await GetActiveWindow().ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, metroDialogSettings);
            });
            switch (result)
            {
                case MessageDialogResult.Negative:
                    return false;
                case MessageDialogResult.Affirmative:
                    return true;
                default:
                    return null;
            }
        }

        public override Task<string> ShowInputDialog(string title, string message)
        {
            return GetActiveWindow().ShowInputAsync(title, message);
        }

        public override bool ShowOpenDialog(out string path, string defaultExt, string filter, string initialDir = null)
        {
            bool? res = null;
            string lPath = null;

            Execute.OnUIThread(() =>
            {
                var ofd = new OpenFileDialog
                          {
                              DefaultExt = defaultExt, Filter = filter,
                          };

                if (initialDir != null)
                {
                    ofd.InitialDirectory = initialDir;
                }

                if (Application.Current.MainWindow != null)
                {
                    res = ofd.ShowDialog(Application.Current.MainWindow);
                }
                else
                {
                    res = ofd.ShowDialog();
                }
                if (res == true)
                {
                    lPath = ofd.FileName;
                }
                else
                {
                    res = false;
                }
            });

            path = lPath;

            return res.Value;
        }
    }
}