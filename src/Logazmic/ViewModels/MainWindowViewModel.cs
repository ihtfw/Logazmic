namespace Logazmic.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    using Caliburn.Micro;

    using Logazmic.Core.Reciever;
    using Logazmic.Properties;
    using Logazmic.Services;

    using MahApps.Metro;
    using MahApps.Metro.Actions;
    using MahApps.Metro.Controls;

    public class MainWindowViewModel : Conductor<LogPaneViewModel>.Collection.OneActive, IDisposable
    {
        private static int i;

        public MainWindowViewModel()
        {
            DisplayName = "Logazmic";
            LoadRecivers();
        }

        private void LoadRecivers()
        {
            Items.Add(new LogPaneViewModel(new TcpReceiver())
                      {
                          DisplayName = "TCP",
                          ContentId = "tcp",
                          CanClose = false
                      });
        }

        public bool IsSettingsOpen { get; set; }

        public void Dispose()
        {
            foreach (var item in Items)
            {
                item.Dispose();
            }
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

                var name = Path.GetFileNameWithoutExtension(path) + "_" + i;
                var paneViewModel = new LogPaneViewModel(new FileReceiver
                                                               {
                                                                   FileToWatch = path,
                                                                   FileFormat = FileReceiver.FileFormatEnums.Log4jXml,
                                                                   ShowFromBeginning = true
                                                               })
                                    {
                                        DisplayName = name,
                                        ToolTip = path,
                                        ContentId = name.ToLower(),
                                        CanClose = true
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
            pane.Dispose();
            Items.Remove(pane);
        }


        #endregion

        protected override void OnActivate()
        {
            base.OnActivate();
            if(Items.Any())
                ActivateItem(Items.First());
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
    }
}