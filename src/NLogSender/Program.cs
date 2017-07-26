using System;
using System.IO;
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
        class FloodContext
        {
            /// <summary>
            /// Just an instance counter.
            /// </summary>
            private static int _counter;

            /// <summary>
            /// Number of loop hits.
            /// </summary>
            private readonly int _numberOfHits;

            /// <summary>
            /// Cooldown after each loop iteration in miliseconds.
            /// </summary>
            private readonly int _hitCooldown;

            /// <summary>
            /// Logger for concrete instance.
            /// </summary>
            private readonly Logger _localLogger = LogManager.GetLogger($"Flooder {_counter++}");

            public FloodContext(int numberOfHits, int hitCooldown)
            {
                _numberOfHits = numberOfHits;
                _hitCooldown = hitCooldown;
            }

            public void Do()
            {
                for (int i = 0; i < _numberOfHits; i++)
                {
                    switch (i % 3)
                    {
                        case 0:
                            _localLogger.Trace(Messages.HugeMessage);
                            break;

                        case 1:
                            _localLogger.Debug(Messages.LargeMessage);
                            break;

                        case 2:
                            _localLogger.Error(Messages.ExceptionMessage, new IOException());
                            break;

                        default:
                            _localLogger.Info(i);
                            break;
                    }
                    Thread.Sleep(_hitCooldown);
                }

                _localLogger.Warn($"{_localLogger.Name} done!");
            }
        }

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

            var numberOfHits = 10000;
            var cooldown = 20;

            var numberOfThreads = Environment.ProcessorCount;

            var flooders = Enumerable.Range(1, numberOfThreads)
                .Select(_ => new FloodContext(numberOfHits, cooldown))
                .Select(f => Task.Factory.StartNew(f.Do))
                .ToArray();

            Task.WaitAll(flooders);
        }
    }
}
