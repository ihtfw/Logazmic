namespace Logazmic.Integration
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class InstallationChecker
    {
        public bool IsInstalled()
        {
            if (!Directory.Exists(LogazmicDir))
            {
                return false;
            }

            if (!Directory.Exists(Path.Combine(LogazmicDir, "packages")))
            {
                return false;
            }

            if (File.Exists(Path.Combine(LogazmicDir, ".dead")))
            {
                return false;
            }

            if (!File.Exists(UpdatePath))
            {
                return false;
            }

            return true;
        }

        public string LogazmicDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Logazmic");

        public string UpdatePath => Path.Combine(LogazmicDir, "Update.exe");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathToSetup"></param> 
        /// <exception cref="LogazmicIntegrationException"></exception>
        public void Install(string pathToSetup)
        {
            if (!File.Exists(pathToSetup))
                throw new LogazmicIntegrationException("Setup file not found");

            var process = Process.Start(pathToSetup);
            process.WaitForExit();
        }
    }
}