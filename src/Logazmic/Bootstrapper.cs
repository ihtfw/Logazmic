using System.Runtime.InteropServices;
using System.Windows.Threading;
using Logazmic.Services;

namespace Logazmic
{
    using Caliburn.Micro;
    using ViewModels;

    public class Bootstrapper : BootstrapperBase
    {
        private bool wasException = false;
        public Bootstrapper()
        {
            Initialize();
            
            IoC.Get<IWindowManager>().ShowWindow(MainWindowViewModel.Instance);
        }

        protected override void Configure()
        {
            base.Configure();
            SetupCaliburnShortcutMessage();
        }

        private static void SetupCaliburnShortcutMessage()
        {
            var currentParser = Parser.CreateTrigger;
            Parser.CreateTrigger = (target, triggerText) =>
            {
                if (ShortcutParser.CanParse(triggerText))
                {
                    return ShortcutParser.CreateTrigger(triggerText);
                }

                return currentParser(target, triggerText);
            };
        }
        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (wasException)
            {
                base.OnUnhandledException(sender, e);
                return;
            }

            wasException = true;

            var msg = e.Exception.Message;
            if (string.IsNullOrEmpty(msg))
            {
                base.OnUnhandledException(sender, e);
                return;
            }

            if (e.Exception is System.InvalidOperationException
                && (msg.Contains("Нельзя задать Visibility или вызвать Show,") ||
                    msg.Contains("Cannot set Visibility or call Show,")))
            {
                e.Handled = true;
            }
            else if (msg.Contains("No target found for method"))
            {
                DialogService.Current.ShowErrorMessageBox(e.Exception);
                e.Handled = true;
            }
            else if (e.Exception is COMException && msg.Contains("OpenClipboard"))
            {
                e.Handled = true;
            }
            else if (e.Exception is System.ArgumentException
                     && msg.Contains("composition")
                     && (msg.Contains("уже завершен") || msg.Contains("has already finished")))
            {
                e.Handled = true;
            }

            if (e.Handled)
            {
                wasException = false;
            }
            base.OnUnhandledException(sender, e);
        }
    }
}