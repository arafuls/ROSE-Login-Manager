using NLog.Config;
using NLog.Targets;
using NLog;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using System.Windows;
using System.IO;



namespace ROSE_Login_Manager
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
                LogManager.GetCurrentClassLogger().Fatal("Another instance of the ROSE Login Manager is already running.");
                _mutex.Dispose();
                _mutex = null;
                Environment.Exit(1);
            }

            // Continue with application initialization
            base.OnStartup(e);

            // NLog configuration
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{GlobalVariables.APP_NAME}", "logs", "${shortdate}.log"),
                Layout = "${longdate} ${uppercase:${level}} ${message}"
            };
            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);
            LogManager.Configuration = config;

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
            Exception? ex = e.Exception;
            LogManager.GetCurrentClassLogger().Fatal(ex, "Unhandled Dispatcher Exception occurred");
            e.Handled = true; // Mark the exception as handled to prevent application shutdown
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
