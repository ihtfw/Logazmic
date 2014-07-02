namespace Logazmic.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;

    using Caliburn.Micro;

    using Logazmic.Core.Log;
    using Logazmic.Core.Reciever;
    using Logazmic.Utils;

    public class LogPaneViewModel : Screen, ILogMessageNotifiable, IHandle<RefreshEvent>, IHandle<RefreshCheckEvent>,
        IDisposable
    {
        private CollectionViewSource collectionViewSource;

        private List<string> logSourceLeaves = new List<string>();

        private LogLevel minLogLevel;

        private string searchString;

        public LogPaneViewModel(Func<IReceiver> receiverFunc)
        {
            LogMessages = new BindableCollection<LogMessage>();
            ReceiverFunc = receiverFunc;
            MinLogLevel = LogLevel.Trace;
            CanClose = true;
            LogSourceRoot = new LogSource
                            {
                                Name = "Root"
                            };
            Messaging.Subscribe(this);

            Init();
        }

        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                Messaging.Publish(new RefreshEvent());
            }
        }

        public bool IsInited { get; private set; }

        public bool IsLoading { get; private set; }

        public bool CanInit { get { return !IsInited && !IsLoading; } }

        public LogSource LogSourceRoot { get; private set; }

        public bool CanClose { get; set; }

        public Func<IReceiver> ReceiverFunc { get; private set; }

        public IReceiver Receiver { get; private set; }

        public BindableCollection<LogMessage> LogMessages { get; private set; }

        public LogMessage SelectedLogMessage { get; set; }

        public string ToolTip { get; set; }

        public string ContentId { get; set; }

        public LogLevel MinLogLevel
        {
            get { return minLogLevel; }
            set
            {
                minLogLevel = value;
                Messaging.Publish(new RefreshEvent());
            }
        }

        public CollectionViewSource CollectionViewSource
        {
            get
            {
                if (collectionViewSource == null)
                {
                    collectionViewSource = new CollectionViewSource();
                    collectionViewSource.Filter += LogMessagesViewSourceFilter;
                    var binding = new Binding
                                  {
                                      IsAsync = true,
                                      Source = this,
                                      Mode = BindingMode.OneWay,
                                      NotifyOnSourceUpdated = true,
                                      Path = new PropertyPath("LogMessages")
                                  };
                    BindingOperations.SetBinding(collectionViewSource, CollectionViewSource.SourceProperty, binding);
                }
                return collectionViewSource;
            }
        }

        public void Dispose()
        {
            if (Receiver != null)
            {
                Receiver.Terminate();
                Receiver.Detach();
            }
            Receiver = null;
        }

        public void Handle(RefreshCheckEvent message)
        {
            logSourceLeaves = LogSourceRoot.Leaves().Where(l => l.IsChecked).Select(c => c.Name).Distinct().ToList();
            Update();
        }

        public void Handle(RefreshEvent message)
        {
            Update();
        }

        public void Clear()
        {
            LogMessages.Clear();
            Update();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            Init();
        }

        public async void Init()
        {
            if (!CanInit)
            {
                return;
            }

            IsLoading = true;
            try
            {
                await Task.Factory.StartNew(() =>
                                            {
                                                if (Receiver != null)
                                                {
                                                    Receiver.Terminate();
                                                    Receiver.Detach();
                                                }

                                                Receiver = ReceiverFunc();

                                                Receiver.Initialize();
                                                Receiver.Attach(this);
                                                IsInited = true;
                                            });
            }
            catch (Exception e)
            {
                IsInited = false;
                Dialogs.ShowErrorMessageBox(e);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LogMessagesViewSourceFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;

            var resultRow = e.Item as LogMessage;
            if (resultRow == null)
            {
                return;
            }

            if (resultRow.LogLevel < MinLogLevel)
            {
                return;
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resultRow.Message, SearchString, CompareOptions.IgnoreCase) < 0)
                {
                    return;
                }
            }

            //            if (LogSourceRoot.Checked.All(c => c != resultRow.LastLoggerName))
            //            {
            //                return;
            //            }

            e.Accepted = true;
        }

        protected void Update()
        {
            Execute.OnUIThread(() =>
                               {
                                   try
                                   {
                                       CollectionViewSource.View.Refresh();
                                   }
                                   catch
                                   {
                                   }
                               });
        }

        #region ILogMessageNotifiable

        public void Notify(LogMessage[] logMsgs)
        {
            LogMessages.AddRange(logMsgs);
            Array.ForEach(logMsgs, m => LogSourceRoot.Find(m.LoggerNames));
        }

        public void Notify(LogMessage logMsg)
        {
            LogMessages.Add(logMsg);

            LogSourceRoot.Find(logMsg.LoggerNames);
        }

        #endregion
    }
}