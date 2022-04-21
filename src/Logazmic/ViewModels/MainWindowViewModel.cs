using System.Net;
using Logazmic.Behaviours;
using Logazmic.Core.Filters;
using Logazmic.Core.Readers;
using Logazmic.Core.Receiver;
using NLog;

namespace Logazmic.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using Caliburn.Micro;
    using Services;
    using Settings;
    using MahApps.Metro.Controls;

    using Squirrel;

    public sealed class MainWindowViewModel : Conductor<LogPaneViewModel>.Collection.OneActive, IDisposable
    {
        private readonly ILogReaderFactory _logReaderFactory;
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        #region Singleton

        private static readonly Lazy<MainWindowViewModel> LazyInstance = new Lazy<MainWindowViewModel>(CreateMainWindowViewModel);

        private static MainWindowViewModel CreateMainWindowViewModel()
        {
            var logReaderFactory = new LogReaderFactory();
            return new MainWindowViewModel(logReaderFactory);
        }

        public static MainWindowViewModel Instance => LazyInstance.Value;

        #endregion

        private MainWindowViewModel(ILogReaderFactory logReaderFactory)
        {
            _logReaderFactory = logReaderFactory;
            DisplayName = "Logazmic";
            LoadReciversFromSettings();
        }


        public string Version { get; set; }

        public bool IsSettingsOpen { get; set; }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            CheckAutoUpdateSettings();
            Version = await CheckForUpdates();
        }

        private void CheckAutoUpdateSettings()
        {
            Execute.OnUIThreadAsync(async () =>
            {
                if (LogazmicSettings.Instance.AutoUpdate == null)
                {
                    var result = await DialogService.Current.ShowQuestionMessageBox("Autoupdate", "Enable autoupdate?");
                    LogazmicSettings.Instance.AutoUpdate = result;
                }
            });
        }
        
        private async Task<string> CheckForUpdates()
        {
            //hack to fix exception: The request was aborted: Could not create SSL/TLS secure channel
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            UpdateManager gitHubManager;
            try
            {
                gitHubManager = await UpdateManager.GitHubUpdateManager("https://github.com/ihtfw/Logazmic", "Logazmic", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to create GitHubUpdateManager");
                //not deployed
                return "Not deployed";
            }

            try
            {
                var currentlyInstalledVersion = gitHubManager.CurrentlyInstalledVersion();
                if (currentlyInstalledVersion == null)
                {
                    return "Not deployed";
                }

                var releaseEntry = await gitHubManager.UpdateApp(progress =>
                {
                    Version = "Updating... " + progress + "%";
                });
                if (releaseEntry != null)
                {
                    return currentlyInstalledVersion + " => " + releaseEntry.Version;
                }

                return currentlyInstalledVersion.ToString();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to update");

                DialogService.Current.ShowErrorMessageBox("Failed to update: " + e.Message);
            }
            finally
            {
                gitHubManager.Dispose();
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

        public void AddReceiver(ReceiverBase receiver)
        {
            receiver.LogReaderFactory = _logReaderFactory;
            if (string.IsNullOrEmpty(receiver.LogFormat))
            {
                receiver.LogFormat = LogazmicSettings.Instance.LogFormat;
            }

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

            if (close)
            {
                LogazmicSettings.Instance.Save();
                GridSplitterSizes.Instance.Save();
                FiltersProfiles.Instance.Save();
            }
        }

        private void OnTabDeactivated(object sender, DeactivationEventArgs args)
        {
            if (!args.WasClosed) return;

            var pane = (LogPaneViewModel)sender;
            //Items.Remove(pane);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    LogazmicSettings.Instance.Receivers.Remove(pane.Receiver);
                    pane.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to remove or dispose receiver");
                }
            });
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

        public void CloseAllButThis(LogPaneViewModel pane)
        {
            if (pane == null) return;

            foreach (var logPaneViewModel in Items.Where(i => i != pane).ToList())
            {
                DeactivateItem(logPaneViewModel, true);
            }
        }

        public void CloseAllTabsOnTheLeft(LogPaneViewModel pane)
        {
            if (pane == null) return;

            foreach (var logPaneViewModel in Items.TakeWhile(i => i != pane).ToList())
            {
                DeactivateItem(logPaneViewModel, true);
            }
        }

        public void CloseAllTabsOnTheRight(LogPaneViewModel pane)
        {
            if (pane == null) return;

            foreach (var logPaneViewModel in Items.SkipWhile(i => i != pane).Skip(1).ToList())
            {
                DeactivateItem(logPaneViewModel, true);
            }
        }

        public async Task AddTCPReceiver()
        {
            var result = await DialogService.Current.ShowInputDialog("TCP Receiver", "Enter port:");
            if (result != null)
            {
                if (!ushort.TryParse(result, out var port))
                {
                    DialogService.Current.ShowErrorMessageBox("Wrong port");
                    return;
                }
                AddReceiver(new TcpReceiver { Port = port, DisplayName = $"TCP({port})" });
            }
        }

        public async Task AddUDPReceiver()
        {
            var result = await DialogService.Current.ShowInputDialog("UDP Receiver", "Enter port:");
            if (result != null)
            {
                if (!ushort.TryParse(result, out var port))
                {
                    DialogService.Current.ShowErrorMessageBox("Wrong port");
                    return;
                }
                AddReceiver(new UdpReceiver { Port = port, DisplayName = $"UDP({port})" });
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

                var fileReceiver = new FileReceiver
                {
                    FileToWatch = path,
                    LogFormat = _logReaderFactory.GetLogFormatByFileExtension(Path.GetExtension(path))
                };
                AddReceiver(fileReceiver);
            }
            catch (Exception e)
            {
                DialogService.Current.ShowErrorMessageBox(e);
            }
        }

        public void Open()
        {
            var res = DialogService.Current.ShowOpenDialog(out var path, ".log4j", "Nlog log4jxml|*.log4jxml;*.log4j|Flat|*.log");
            if (!res)
            {
                return;
            }

            LoadFile(path);
        }

        public void ExcludeLogEntry()
        {
            ActiveItem?.ProfileFiltersViewModel.AddMessageFilter(ActiveItem?.SelectedLogMessage?.Message);
        }

        public void ScrollIntoSelected()
        {
            ActiveItem?.ScrollIntoSelected(true);
        }

        public void Clear()
        {
            ActiveItem?.Clear();
        }

        public void FindNext()
        {
            ActiveItem?.FindNext();
        }

        public void CloseActiveTab()
        {
            ActiveItem?.TryClose();
        }

        public void CloseTab(BaseMetroTabControl.TabItemClosingEventArgs args)
        {
            var pane = (LogPaneViewModel)args.ClosingTabItem.Content;
            DeactivateItem(pane, true);
        }
       
        #endregion

        public void Dispose()
        {
            foreach (var item in Items)
            {
                item.Dispose();
            }
        }
    }
}