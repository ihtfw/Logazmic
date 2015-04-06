namespace Logazmic
{
    using Caliburn.Micro;

    using Logazmic.ViewModels;

    public class Bootstrapper : BootstrapperBase
    {
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
    }
}