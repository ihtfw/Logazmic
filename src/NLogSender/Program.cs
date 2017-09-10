using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;

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
                    switch (i % 4)
                    {
                        case 0:
                            _localLogger.Trace($"{i}: {Messages.HugeMessage}");
                            break;

                        case 1:
                            _localLogger.Debug($"{i}: {Messages.LargeMessage}");
                            break;

                        case 2:
                            _localLogger.Error($"{i}: {Messages.ExceptionMessage}", new IOException());
                            break;

                        default:
                            _localLogger.Info(i);
                            break;
                    }
                    Thread.Sleep(_hitCooldown);
                }

                Console.Out.WriteLine($"{_localLogger.Name} done!");
            }
        }

        static OptionSet _optionSet;

        public static void SetupNLog(int port = 4505)
        {
            var config = new LoggingConfiguration();
            
            var tcpNetworkTarget = new NLogViewerTarget
            {
                Address = $"tcp4://127.0.0.1:{port}",
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
            var numberOfHits = 1000;
            var cooldown = 20;
            var numberOfThreads = 1;//Environment.ProcessorCount;
            var port = 4505;

            _optionSet = new OptionSet
            {
                {
                    "t|threads=", $"Number of flood thread. {numberOfThreads} by-default.",
                    t => { int.TryParse(t, out numberOfThreads); }
                },
                {
                    "m|messages=", $"Number of messages per thread. {numberOfHits} by-default.", h =>
                    {
                        int.TryParse(h, out numberOfHits);
                    }
                },
                {
                    "c|cooldown=", $"Cooldown after each message in miliseconds. {cooldown} by-default", c =>
                    {
                        int.TryParse(c, out cooldown);
                    }
                },
                {
                    "p|port=", $"TCP port. Default is {port}", p =>
                    {
                        int.TryParse(p, out port);
                    }
                },
                {
                    "h|help", "Show help", _ =>
                    {
                        _optionSet.WriteOptionDescriptions(Console.Out);
                        Environment.Exit(0);
                    }
                }
            };
            _optionSet.Parse(args);

            SetupNLog();

            Console.Out.WriteLine($"Starting flood with\n\tPort:\t{port}\n\tThreads:\t{numberOfThreads}\n\tMessages per thread:\t{numberOfHits}\n\tCooldown after message:\t{cooldown} milisec");

            var flooders = Enumerable.Range(1, numberOfThreads)
                .Select(_ => new FloodContext(numberOfHits, cooldown))
                .Select(f => Task.Factory.StartNew(f.Do))
                .ToArray();

            Task.WaitAll(flooders);
        }
    }
}
