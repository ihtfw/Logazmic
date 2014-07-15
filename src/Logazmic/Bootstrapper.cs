namespace Logazmic
{
    using System.Windows;

    using Caliburn.Micro;

    using Logazmic.ViewModels;

    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
            IoC.Get<IWindowManager>().ShowWindow(MainWindowViewModel.Instance);
        }
    }
}