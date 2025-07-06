using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using ROSE_Login_Manager.ViewModels;
using Wpf.Ui.Controls;

namespace ROSE_Login_Manager.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ILogger<MainWindowViewModel> _logger;

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger)
        {
            _logger = logger;
            InitializeNavigationItems();
        }

        [ObservableProperty]
        private string _applicationTitle = "Rose Online Login Manager";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new();

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Profiles", Tag = "tray_profiles" },
            new MenuItem { Header = "Settings", Tag = "tray_settings" },
            new MenuItem { Header = "Exit", Tag = "tray_exit" }
        };

        private void InitializeNavigationItems()
        {
            MenuItems.Clear();
            MenuItems.Add(new NavigationViewItem()
            {
                Content = "Patcher",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ArrowDownload24 },
                TargetPageType = typeof(Views.Pages.PatcherPage)
            });

            MenuItems.Add(new NavigationViewItem()
            {
                Content = "Profiles",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Person24 },
                TargetPageType = typeof(Views.Pages.ProfilePage)
            });

            FooterMenuItems.Clear();
            FooterMenuItems.Add(new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            });

            _logger.LogInformation("Navigation items initialized");
        }
    }
}
