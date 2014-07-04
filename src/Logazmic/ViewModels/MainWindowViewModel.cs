namespace Logazmic.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;

    using Caliburn.Micro;

    using Logazmic.Core.Reciever;
    using Logazmic.Services;
    using Logazmic.Settings;

    using MahApps.Metro.Controls;

    public class MainWindowViewModel : Conductor<LogPaneViewModel>.Collection.OneActive, IDisposable
    {
        private static int i;

        public MainWindowViewModel()
        {
            DisplayName = "Logazmic";
            LoadRecivers();
        }

        public bool IsSettingsOpen { get; set; }

        public void Dispose()
        {
            foreach (var item in Items)
            {
                item.Dispose();
            }
        }

        private void LoadRecivers()
        {
            foreach (var receiver in LogazmicSettings.Instance.Receivers)
            {
                Items.Add(new LogPaneViewModel(receiver));
            }
        }

        public void AddReceiver(IReceiver receiver)
        {
            LogazmicSettings.Instance.Receivers.Add(receiver);
            Items.Add(new LogPaneViewModel(receiver));
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null &&
                AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null &&
                AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Any())
            {
                string[] activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                var uri = new Uri(activationData[0]);

                LoadFile(uri.LocalPath);
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

               
                var paneViewModel = new LogPaneViewModel(new FileReceiver(path){
                                                             FileFormat = FileReceiver.FileFormatEnums.Log4jXml,
                                                             })
                                    {
                                        ToolTip = path,
                                    };
                i++;
                Items.Add(paneViewModel);

                ActivateItem(paneViewModel);
            }
            catch (Exception e)
            {
                DialogService.Current.ShowErrorMessageBox(e);
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            if (Items.Any())
            {
                ActivateItem(Items.First());
            }
        }

        public void Test()
        {
            DialogService.Current.ShowErrorMessageBox(new ApplicationException("123"));
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Dispose();
            }
            base.OnDeactivate(close);
        }

        #region Actions

        public void Clear()
        {
            try
            {
                if (ActiveItem == null)
                {
                    return;
                }

                ActiveItem.Clear();
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
            LogazmicSettings.Instance.Receivers.Remove(pane.Receiver);
            pane.Dispose();
            Items.Remove(pane);
        }

        #endregion
    }
}