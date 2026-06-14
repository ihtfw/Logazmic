using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Threading;
using Logazmic.Services;
using Logazmic.Settings;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Logazmic
{
    using Caliburn.Micro;
    using ViewModels;

    public class Bootstrapper : BootstrapperBase
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _wasException;

        /// <summary>
        /// Gets the directory where log files are stored.
        /// </summary>
        public static string LogDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Logazmic", "logs");

        public Bootstrapper()
        {
            Initialize();
            
            IoC.Get<IWindowManager>().ShowWindow(MainWindowViewModel.Instance);
        }

        protected override void Configure()
        {
            base.Configure();
            SetupCaliburnShortcutMessage();
            SetupLogging();
            LogStartupInfo();
        }

        /// <summary>
        /// Configures NLog with a file target (always-on) and an optional TCP self-logging target.
        /// </summary>
        private static void SetupLogging()
        {
            var config = new LoggingConfiguration();

            #region File target (always-on)

            var logDirectory = LogDirectory;
            try
            {
                Directory.CreateDirectory(logDirectory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create log directory '{logDirectory}': {ex.Message}");
                return;
            }

            var fileTarget = new FileTarget
            {
                Name = "LogFile",
                FileName = Path.Combine(logDirectory, "Logazmic-${shortdate}.log4j"),
                Layout = " ${log4jxmlevent}",
                KeepFileOpen = false,
                Encoding = Encoding.UTF8,
                ArchiveAboveSize = 10 * 1024 * 1024, // 10 MB
                MaxArchiveFiles = 7,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                ConcurrentWrites = true
            };
            config.AddTarget(fileTarget);

            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(fileRule);

            #endregion

            #region TCP self-logging target (opt-in)

            if (LogazmicSettings.Instance.EnableSelfLogging)
            {
                var tcpNetworkTarget = new NLogViewerTarget
                {
                    Address = "tcp4://127.0.0.1:" + LogazmicSettings.Instance.SelfLoggingPort,
                    Encoding = Encoding.UTF8,
                    Name = "NLogViewer",
                    IncludeNLogData = false
                };
                config.AddTarget(tcpNetworkTarget);

                var tcpNetworkRule = new LoggingRule("*", LogLevel.Trace, tcpNetworkTarget);
                config.LoggingRules.Add(tcpNetworkRule);
            }

            #endregion

            NLog.LogManager.Configuration = config;
        }

        /// <summary>
        /// Logs structured startup context for diagnostics.
        /// </summary>
        private static void LogStartupInfo()
        {
            try
            {
                Logger.Info("=== Logazmic Starting ===");
                Logger.Info("App Version: {0}", typeof(Bootstrapper).Assembly.GetName().Version);
                Logger.Info("OS: {0}", Environment.OSVersion);
                Logger.Info("64-bit OS: {0}", Environment.Is64BitOperatingSystem);
                Logger.Info("64-bit Process: {0}", Environment.Is64BitProcess);
                Logger.Info("CLR Version: {0}", Environment.Version);
                Logger.Info("Processor Count: {0}", Environment.ProcessorCount);
                Logger.Info("Working Set: {0:N0} bytes", Environment.WorkingSet);
                Logger.Info("Settings: DarkTheme={0}, LogFormat={1}, SingleWindow={2}, AutoUpdate={3}",
                    LogazmicSettings.Instance.UseDarkTheme,
                    LogazmicSettings.Instance.LogFormat,
                    LogazmicSettings.Instance.SingleWindowMode,
                    LogazmicSettings.Instance.AutoUpdate);
                Logger.Info("SelfLogging: Enabled={0}, Port={1}",
                    LogazmicSettings.Instance.EnableSelfLogging,
                    LogazmicSettings.Instance.SelfLoggingPort);
                Logger.Info("Log Directory: {0}", LogDirectory);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to log startup info");
            }
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

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (_wasException)
            {
                Logger.Error(e.Exception, "Recursive unhandled exception");
                base.OnUnhandledException(sender, e);
                return;
            }

            _wasException = true;

            var msg = e.Exception.Message;
            if (string.IsNullOrEmpty(msg))
            {
                base.OnUnhandledException(sender, e);
                return;
            }

            // Always log unhandled exceptions to the file log for diagnostics
            Logger.Error(e.Exception, "Unhandled exception: {0}", msg);

            if (e.Exception is System.InvalidOperationException
                && (msg.Contains("Нельзя задать Visibility или вызвать Show,") ||
                    msg.Contains("Cannot set Visibility or call Show,")))
            {
                e.Handled = true;
            }
            else if (msg.Contains("No target found for method"))
            {
                DialogService.Current.ShowErrorMessageBox(e.Exception);
                e.Handled = true;
            }
            else if (e.Exception is COMException && msg.Contains("OpenClipboard"))
            {
                e.Handled = true;
            }
            else if (e.Exception is System.ArgumentException
                     && msg.Contains("composition")
                     && (msg.Contains("уже завершен") || msg.Contains("has already finished")))
            {
                e.Handled = true;
            }

            if (e.Handled)
            {
                _wasException = false;
            }
            base.OnUnhandledException(sender, e);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            Logger.Info("=== Logazmic Shutting Down ===");
            base.OnExit(sender, e);
        }
    }
}