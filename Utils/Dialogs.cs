namespace Logazmic.Utils
{
    using System;
    using System.Windows;

    using Caliburn.Micro;

    using Microsoft.Win32;

    static class Dialogs
    {
        public static void ShowErrorMessageBox(Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
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
