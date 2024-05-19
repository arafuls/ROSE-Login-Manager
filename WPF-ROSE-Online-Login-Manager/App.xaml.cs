using System.Configuration;
using System.Data;
using System.Windows;

namespace ROSE_Online_Login_Manager
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
            const string mutexName = "ROSE_Online_Login_Manager_Mutex"; // Replace with your application name

            // Attempt to create a new mutex
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            // If the mutex already exists, exit the application
            if (!createdNew)
            {
                MessageBox.Show(
                    "Another instance of the application is already running.", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _mutex.Dispose();
                _mutex = null;
                Environment.Exit(1);
            }

            // Continue with application initialization
            base.OnStartup(e);

            ROSE_Online_Login_Manager.Model.ConfigurationManager.Instance.LoadConfig();
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
