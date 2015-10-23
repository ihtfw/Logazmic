namespace Logazmic
{
    using System;

    using Caliburn.Micro;

    using Logazmic.Services;
    using Logazmic.ViewModels;

    using Squirrel;

    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
            IoC.Get<IWindowManager>().ShowWindow(MainWindowViewModel.Instance);
            CheckForUpdates();
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

        private async void CheckForUpdates()
        {
            try
            {
                using (var gitHubManager = await UpdateManager.GitHubUpdateManager("https://github.com/ihtfw/Logazmic"))
                {
                    if (gitHubManager.IsInstalledApp)
                    {
                        var releaseEntry = await gitHubManager.UpdateApp();
                    }
                }
            }
            catch (Exception e)
            {
                DialogService.Current.ShowErrorMessageBox("Failed to update: " + e.Message);
            }

        }
    }
}