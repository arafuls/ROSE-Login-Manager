﻿using NLog;
using NLog.Config;
using NLog.Targets;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Logging;
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
            // Configure logging before performing other operations
            ConfigureLogging();

            // Handle application-wide exceptions
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            // Attempt to ensure only a single instance is running
            if (!TryAcquireMutex())
            {
                Logger.Warn("Another instance of the ROSE Login Manager is already running.");
                ShutdownApplication();
                return; // Prevent further processing
            }

            base.OnStartup(e);

            // Instantiate Singletons
            _ = GlobalVariables.Instance;
            _ = ConfigurationManager.Instance;
            _ = ProcessManager.Instance;
        }



        /// <summary>
        ///     Handles unhandled exceptions occurring in the Dispatcher thread.
        ///     Logs the exception using NLog at the Fatal level.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">DispatcherUnhandledExceptionEventArgs that contains the event data.</param>
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e.Exception, "Unhandled Dispatcher Exception occurred.");
            e.Handled = true; // Mark the exception as handled to prevent application shutdown
        }



        /// <summary>
        ///     Tries to acquire a named mutex to ensure only one instance of the application is running.
        /// </summary>
        /// <returns>True if the mutex was created; false if it already existed.</returns>
        private static bool TryAcquireMutex()
        {
            const string mutexName = "ROSE_Login_Manager_Mutex";

            // Create or open the mutex
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                // Mutex already exists
                _mutex.Dispose();
                _mutex = null;
                return false;
            }

            return true;
        }



        /// <summary>
        ///     Disposes of the mutex and shuts down the application with an exit code of 1.
        /// </summary>
        /// <remarks>Releases the mutex and terminates the application, indicating an abnormal exit.</remarks>
        private static void ShutdownApplication()
        {
            // Ensure mutex is disposed properly before exiting
            _mutex?.Dispose();
            _mutex = null;
            Application.Current.Shutdown(1);
        }



        /// <summary>
        ///     Configures the logging settings for the application.
        /// </summary>
        private static void ConfigureLogging()
        {
            LoggingConfiguration config = new();
            FileTarget fileTarget = new("file")
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{GlobalVariables.APP_NAME}", "logs", "${shortdate}.log"),
                Layout = "${longdate} ${uppercase:${level}} ${callsite:className=true:methodName=true} ${message} ${exception}"
            };

            // Create an instance of the custom log target
            var logCollectorTarget = new LogCollectorTarget
            {
                Name = "logCollector",
                Layout = "${longdate} ${uppercase:${level}} ${callsite:className=true:methodName=true} ${message} ${exception:format=tostring}"
            };

            // Add targets to the configuration
            config.AddTarget(fileTarget);
            config.AddTarget(logCollectorTarget);

            // Configure rules
            if (System.Diagnostics.Debugger.IsAttached)
            {
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logCollectorTarget);
            }
            else
            {
                config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
                config.AddRule(LogLevel.Info, LogLevel.Fatal, logCollectorTarget);
            }

            // Apply configuration
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
