using System;
using System.Threading.Tasks;

namespace Logazmic.Integration
{
    using System.Diagnostics;
    using System.IO;
    using System.Net;

    public class Runner
    {
        public Runner()
        {
            DirectoryToDownloadSetup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Logazmic");
        }

        /// <summary>
        /// 
        /// </summary>
        public WebProxy WebProxy { get; set; }

        /// <summary>
        /// Default dir is %appdata%/Logazmic
        /// </summary>
        public string DirectoryToDownloadSetup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathToLogFile">Log file to open</param>
        /// <returns></returns>
        /// <exception cref="LogazmicIntegrationException"></exception>
        public async Task Run(string pathToLogFile = null)
        {
            if (!Directory.Exists(DirectoryToDownloadSetup))
            {
                Directory.CreateDirectory(DirectoryToDownloadSetup);
            }

            string pathToSetup = Path.Combine(DirectoryToDownloadSetup, "Setup.exe");
            if (File.Exists(pathToSetup))
            {
                File.Delete(pathToSetup);
            }

            var installationChecker = new InstallationChecker();
            if (!installationChecker.IsInstalled())
            {
                var downloader = new Downloader
                                 {
                                     WebProxy = WebProxy
                                 };

                await downloader.Download(pathToSetup);
                installationChecker.Install(pathToSetup);
            }

            if (!installationChecker.IsInstalled())
            {
                throw new LogazmicIntegrationException("Failed to install Logazmic");
            }

            var arguments = " --processStart Logazmic.exe";
            if (!string.IsNullOrWhiteSpace(pathToLogFile) && File.Exists(pathToLogFile))
            {
                arguments += " --process-start-args " + pathToLogFile;
            }
            Process.Start(installationChecker.UpdatePath, arguments);
        }
    }
}
