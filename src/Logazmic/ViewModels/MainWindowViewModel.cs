namespace Logazmic.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using Caliburn.Micro;

    using Logazmic.Core.Reciever;
    using Logazmic.Services;
    using Logazmic.Settings;
    using Logazmic.Utils;

    using MahApps.Metro.Controls;

    using Squirrel;

    public sealed class MainWindowViewModel : Conductor<LogPaneViewModel>.Collection.OneActive, IDisposable
    {
        #region Singleton

        private static readonly Lazy<MainWindowViewModel> instance = new Lazy<MainWindowViewModel>(() => new MainWindowViewModel());

        public static MainWindowViewModel Instance => instance.Value;

        #endregion

        private MainWindowViewModel()
        {
            DisplayName = "Logazmic";
            LoadReciversFromSettings();
        }

        public string Version { get; set; }

        public bool IsSettingsOpen { get; set; }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            Version = await CheckForUpdates();
        }
        
        private async Task<string> CheckForUpdates()
        {
            try
            {
                using (var gitHubManager = await UpdateManager.GitHubUpdateManager("https://github.com/ihtfw/Logazmic"))
                {
                    var releaseEntry = await gitHubManager.UpdateApp();
                    if (releaseEntry != null)
                    {
                        return gitHubManager.CurrentlyInstalledVersion() + " => " + releaseEntry.Version;
                    }
                    return gitHubManager.CurrentlyInstalledVersion().ToString();
                }
            }
            catch (Exception e)
            {
                if (e.Message != "Update.exe not found, not a Squirrel-installed app?")
                {
                    DialogService.Current.ShowErrorMessageBox("Failed to update: " + e.Message);
                    return "1.0.0";
                }
            }

            return "1.0.0";
        }

        private void LoadReciversFromSettings()
        {
            foreach (var receiver in LogazmicSettings.Instance.Receivers)
            {
                AddReceiver(receiver);
            }
        }

        public void AddReceiver(ReceiverBase receiver, string tooltip = null)
        {
            if (!LogazmicSettings.Instance.Receivers.Contains(receiver))
            {
                LogazmicSettings.Instance.Receivers.Add(receiver);
            }
            var logPaneViewModel = new LogPaneViewModel(receiver);
            logPaneViewModel.Deactivated += OnTabDeactivated;
            Items.Add(logPaneViewModel);
            ActivateItem(logPaneViewModel);
            Task.Factory.StartNew(logPaneViewModel.Initialize).ContinueWith(t =>
            {
                if(t.Exception != null)
                    LogazmicSettings.Instance.Receivers.Remove(receiver);
            });
        }

        protected override void OnDeactivate(bool close)
        {
            foreach (var item in Items)
            {
                item.Deactivated -= OnTabDeactivated;
            }
            base.OnDeactivate(close);
        }

        private void OnTabDeactivated(object sender, DeactivationEventArgs args)
        {
            if (args.WasClosed)
            {
                var pane = (LogPaneViewModel)sender;
                LogazmicSettings.Instance.Receivers.Remove(pane.Receiver);
                pane.Dispose();
                Items.Remove(pane);
            }
        }

        public void OnDrop(DragEventArgs e)
        {
            var dataObject = e.Data as DataObject;
            if (dataObject == null)
            {
                return;
            }

            if (dataObject.ContainsFileDropList())
            {
                var fileNames = dataObject.GetFileDropList();

                foreach (var fileName in fileNames)
                {
                    LoadFile(fileName);
                }
            }
        }

        #region Actions

        public async void AddTCPReciever()
        {
            var result = await DialogService.Current.ShowInputDialog("TCP Reciever", "Enter port:");
            if (result != null)
            {
                ushort port;
                if (!ushort.TryParse(result, out port))
                {
                    DialogService.Current.ShowErrorMessageBox("Wrong port");
                    return;
                }
                AddReceiver(new TcpReceiver { Port = port, DisplayName = string.Format("TCP({0})", port) });
            }
        }

        public async void AddUDPReciever()
        {
            var result = await DialogService.Current.ShowInputDialog("UDP Reciever", "Enter port:");
            if (result != null)
            {
                ushort port;
                if (!ushort.TryParse(result, out port))
                {
                    DialogService.Current.ShowErrorMessageBox("Wrong port");
                    return;
                }
                AddReceiver(new UdpReceiver { Port = port, DisplayName = string.Format("UDP({0})", port) });
            }
        }

        public void LoadFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return;
                }

                var alreadyOpened = Items.FirstOrDefault(it => it.ToolTip == path);
                if (alreadyOpened != null)
                {
                    ActivateItem(alreadyOpened);
                    return;
                }

                AddReceiver(new FileReceiver
                            {
                                FileToWatch = path,
                                FileFormat = FileReceiver.FileFormatEnums.Log4jXml,
                            }, path);
            }
            catch (Exception e)
            {
                DialogService.Current.ShowErrorMessageBox(e);
            }
        }

        public void Open()
        {
            string path;
            var res = DialogService.Current.ShowOpenDialog(out path, ".log4j", "Nlog log4jxml|*.log4jxml;*.log4j");
            if (!res)
            {
                return;
            }

            LoadFile(path);
        }

        public void ExcludeLogEntry()
        {
            ActiveItem?.AddMessageFilter(ActiveItem?.SelectedLogMessage?.Message);
        }

        public void CloseTab(BaseMetroTabControl.TabItemClosingEventArgs args)
        {
            var pane = (LogPaneViewModel)args.ClosingTabItem.Content;
            pane.TryClose();
        }

        #endregion

        public void Dispose()
        {
            foreach (var item in Items)
            {
                item.Dispose();
            }
        }

        public void FindNext()
        {
            ActiveItem?.FindNext();
        }
    }
}