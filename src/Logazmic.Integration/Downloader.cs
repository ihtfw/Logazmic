using System;
using System.Linq;
using System.Threading.Tasks;

namespace Logazmic.Integration
{
    using System.Net;

    public class Downloader
    {
        public WebProxy WebProxy { get; set; }

        /// <summary>
        /// from https://github.com/ihtfw/Logazmic/releases/tag/2015.11.13.4
        /// to https://github.com/ihtfw/Logazmic/releases/download/2015.11.13.4/Setup.exe
        /// </summary>
        /// <param name="latestRelease"></param>
        public string ConvertUrl(string latestRelease)
        {
            var version = latestRelease.Split('/').Last();
            return "https://github.com/ihtfw/Logazmic/releases/download/" + version +"/Setup.exe";
        }

        public async Task<Uri> GetLatestReleaseUrl()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create("https://github.com/ihtfw/Logazmic/releases/latest");
            if (WebProxy != null)
            {
                webRequest.Proxy = WebProxy;
            }
            webRequest.Method = "HEAD";
            using (await webRequest.GetResponseAsync())
            {
            }
            return webRequest.Address;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Path where to download Setup.exe</param>
        /// <returns></returns>
        /// <exception cref="LogazmicIntegrationException"></exception>
        public async Task Download(string fileName)
        {
            string latestReleaseUrl;
            try
            {
                latestReleaseUrl = (await GetLatestReleaseUrl())?.ToString();
            }
            catch (Exception e)
            {
                throw new LogazmicIntegrationException("Failed to get latest release url", e);
            }
            
            if (string.IsNullOrEmpty(latestReleaseUrl))
                throw new LogazmicIntegrationException("Failed to get latest release url. It was null");

            try
            {
                var downloadUrl = ConvertUrl(latestReleaseUrl);
                using (var client = new WebClient())
                {
                    if (WebProxy != null)
                    {
                        client.Proxy = WebProxy;
                    }

                    await client.DownloadFileTaskAsync(downloadUrl, fileName);
                }
            }
            catch (Exception e)
            {
                throw new LogazmicIntegrationException("Failed to download Setup.exe", e);
            }
        }
    }
}
