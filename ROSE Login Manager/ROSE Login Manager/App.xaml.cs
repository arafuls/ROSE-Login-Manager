using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.ViewModels;
using ROSE_Login_Manager.ViewModels.Pages;
using ROSE_Login_Manager.ViewModels.Windows;
using ROSE_Login_Manager.Views.Pages;
using ROSE_Login_Manager.Views.Windows;
using Serilog;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace ROSE_Login_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)); })
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "RoseOnlineLoginManager",
                        "logs",
                        "app-.log"
                    ),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    fileSizeLimitBytes: 10 * 1024 * 1024 // 10MB
                )
            )
            .ConfigureServices((context, services) =>
            {
                services.AddNavigationViewPageProvider();

                services.AddHostedService<ApplicationHostService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                // Register our custom services
                services.AddSingleton<IDatabaseService, DatabaseService>();
                services.AddSingleton<IEncryptionService, EncryptionService>();
                services.AddSingleton<IUserProfileService, UserProfileService>();
                services.AddSingleton<ILauncherSettingsService, LauncherSettingsService>();

                // Register our ViewModels
                services.AddTransient<MainViewModel>();

                services.AddSingleton<ProfilePage>();
                services.AddTransient<ProfileViewModel>();
                services.AddSingleton<PatcherPage>();
                services.AddSingleton<PatcherViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            }).Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                // Initialize database
                var databaseService = Services.GetRequiredService<IDatabaseService>();
                await databaseService.InitializeAsync();

                Log.Information("Application started successfully");
                await _host.StartAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                throw;
            }
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            try
            {
                Log.Information("Application shutting down");
                await _host.StopAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during application shutdown");
            }
            finally
            {
                _host.Dispose();
            }
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "Unhandled exception occurred");
            e.Handled = true;
        }
    }
}
