using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;



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
            const string mutexName = "ROSE_Login_Manager_Mutex"; // Replace with your application name

            // Attempt to create a new mutex
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            // If the mutex already exists, exit the application
            if (!createdNew)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - App::OnStartup",
                    message: "Another instance of the application is already running.",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Information);
                _mutex.Dispose();
                _mutex = null;
                Environment.Exit(1);
            }

            // Continue with application initialization
            base.OnStartup(e);

            // Instantiate Singletons
            _ = GlobalVariables.Instance;
            _ = ConfigurationManager.Instance;

            // ROSE Updater
            _ = new RoseUpdater();
        }



        /// <summary>
        ///     Handles the application exit event.
        /// </summary>
        /// <param name="e">Information about the exit event.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            // Release the mutex when the application exits
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }

}
