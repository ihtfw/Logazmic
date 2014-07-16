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

    public sealed class MainWindowViewModel : Conductor<LogPaneViewModel>.Collection.OneActive, IDisposable
    {
        #region Singleton

        private static readonly Lazy<MainWindowViewModel> instance = new Lazy<MainWindowViewModel>(() => new MainWindowViewModel());

        public static MainWindowViewModel Instance { get { return instance.Value; } }

        #endregion

        private MainWindowViewModel()
        {
            DisplayName = "Logazmic";
            LoadReciversFromSettings();
        }

        public bool IsSettingsOpen { get; set; }

        public void Dispose()
        {
            foreach (var item in Items)
            {
                item.Dispose();
            }
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
            Task.Factory.StartNew(logPaneViewModel.Initialize);
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

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            
            var coa = ClickOnceUtils.StartUpArg;
            if (!string.IsNullOrEmpty(coa))
            {
                LoadFile(coa);
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

                if (Items.Any(it => it.ToolTip == path))
                {
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
            var res = DialogService.Current.ShowOpenDialog(out path, ".log4jxml", "Nlog log4jxml|*.log4jxml");
            if (!res)
            {
                return;
            }

            LoadFile(path);
        }

        public void CloseTab(BaseMetroTabControl.TabItemClosingEventArgs args)
        {
            var pane = (LogPaneViewModel)args.ClosingTabItem.Content;
            pane.TryClose();
            
        }

        #endregion
    }
}