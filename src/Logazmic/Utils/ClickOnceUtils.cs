namespace Logazmic.Utils
{
    using System;
    using System.Linq;

    public static class ClickOnceUtils
    {
        public static string StartUpArg
        {
            get
            {
                if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null &&
                    AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null &&
                    AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Any())
                {
                    string[] activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                    var uri = new Uri(activationData[0]);

                    return uri.LocalPath;
                }

                return null;
            }
        }
    }
}