namespace Logazmic.Utils
{
    using System;
    using System.Linq;
    using System.Windows;

    using Caliburn.Micro;

    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;

    using Microsoft.Win32;

    static class Dialogs
    {
        public static void ShowErrorMessageBox(Exception e)
        {
            var window = Application.Current.Windows.OfType<MetroWindow>().Single(w => w.IsActive);
            window.ShowMessageAsync("Error", e.Message);

        }   

        public static bool ShowOpenDialog(out string path, string defaultExt, string filter, string initialDir = null)
        {
            bool? res = null;
            string lPath = null;

            Execute.OnUIThread(() =>
            {
                var ofd = new OpenFileDialog
                {
                    DefaultExt = defaultExt,
                    Filter = filter,
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
