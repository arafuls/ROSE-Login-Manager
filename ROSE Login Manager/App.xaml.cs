﻿using NLog;
using NLog.Config;
using NLog.Targets;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using System.IO;
using System.Windows;



namespace ROSE_Login_Manager
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static Mutex? _mutex = null;



        /// <summary>
        ///     Handles the application startup event.
        /// </summary>
        /// <param name="e">Information about the startup event.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "ROSE_Login_Manager_Mutex";

            // Attempt to create a new mutex
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            // If the mutex already exists, exit the application
            if (!createdNew)
            {
                Logger.Fatal("Another instance of the ROSE Login Manager is already running.");

                _mutex.Dispose();
                _mutex = null;
                Environment.Exit(1);
            }

            base.OnStartup(e);

            ConfigureLogging();

            // Instantiate Singletons
            _ = GlobalVariables.Instance;
            _ = ConfigurationManager.Instance;
            _ = ProcessManager.Instance;

            // Subscribe to handle unhandled exceptions on the UI (Dispatcher) thread.
            // This ensures that any unhandled exceptions occurring on the UI thread are caught and logged.
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }



        /// <summary>
        ///     Handles unhandled exceptions occurring in the Dispatcher thread.
        ///     Logs the exception using NLog at the Fatal level.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">DispatcherUnhandledExceptionEventArgs that contains the event data.</param>
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e.Exception, "Unhandled Dispatcher Exception occurred");
            e.Handled = true; // Mark the exception as handled to prevent application shutdown
        }



        /// <summary>
        ///     Configures the logging settings for the application.
        /// </summary>
        /// <remarks>
        ///     This method sets up a logging configuration that writes log entries to a file. 
        ///     The log files are saved in a directory specified by the application's name in the user's 
        ///     application data folder, with the filename format based on the current date.
        ///     
        ///     The configuration includes:
        ///     - A <see cref="FileTarget"/> that specifies the file path and format of log entries.
        ///     - A log rule that logs messages from <see cref="LogLevel.Trace"/> to <see cref="LogLevel.Fatal"/>.
        ///     
        ///     The log layout is formatted to include the date, log level, and message, with the level 
        ///     converted to uppercase.
        /// </remarks>
        private static void ConfigureLogging()
        {
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{GlobalVariables.APP_NAME}", "logs", "${shortdate}.log"),
                Layout = "${longdate} ${uppercase:${level}} ${message} ${exception}"
            };

            // Targets
            config.AddTarget(fileTarget);

            // Rules
            if (System.Diagnostics.Debugger.IsAttached)
            {
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
            }
            else
            {
                config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
            }

            LogManager.Configuration = config;
        }



        /// <summary>
        ///     Handles the application exit event.
        /// </summary>
        /// <param name="e">Information about the exit event.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            LogManager.Shutdown();

            _mutex?.ReleaseMutex();
            _mutex?.Dispose();

            base.OnExit(e);
        }
    }

}
