namespace Logazmic.Services
{
    using System.Linq;
    using System.Windows;

    using Caliburn.Micro;

    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;

    using Microsoft.Win32;

    class MetroDialogService : DialogService
    {
        private MetroWindow GetActiveWindow()
        {
            return Application.Current.Windows.OfType<MetroWindow>().Single(w => w.IsActive);
        }

        public override void ShowMessageBox(string title, string message)
        {
            GetActiveWindow().ShowMessageAsync(title, message);
        }

        public override bool ShowOpenDialog(out string path, string defaultExt, string filter, string initialDir = null)
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