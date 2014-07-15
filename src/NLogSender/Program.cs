using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogSender
{
    using System.Threading;

    using NLog;
    using NLog.Config;
    using NLog.Targets;

    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void SetupNLog()
        {
            var config = new LoggingConfiguration();
            
            var tcpNetworkTarget = new NLogViewerTarget
            {
                Address = "tcp4://127.0.0.1:4505",
                Encoding = Encoding.UTF8,
                Name = "NLogViewer",
                IncludeNLogData = false
            };

            var tcpNetworkRule = new LoggingRule("*", LogLevel.Trace, tcpNetworkTarget);
            config.LoggingRules.Add(tcpNetworkRule);

            LogManager.Configuration = config;
        }

        static void Main(string[] args)
        {
            SetupNLog();

            for (int i = 0; i < 1000; i++)
            {
                switch (i % 2)
                {
                    case 0:
                        Logger.Trace(i);
                        break;
                    case 1:
                        Logger.Debug(i);
                        break;
                    default:
                        Logger.Info(i);
                        break;
                }
                Thread.Sleep(10);
            }
        }
    }
}
