namespace Logazmic
{
    using System.Windows;

    using Caliburn.Micro;

    public class LogViewerBootstrapper : BootstrapperBase
    {
        public LogViewerBootstrapper()
        {
            Initialize();
        }
        
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<LogViewerViewModel>();
        }
    }
}
