using Microsoft.Extensions.Logging;
using ROSE_Login_Manager.ViewModels;

namespace ROSE_Login_Manager.ViewModels.Pages
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public ProfileViewModel(MainViewModel mainViewModel, ILogger<ProfileViewModel> logger)
        {
            _mainViewModel = mainViewModel;
            logger.LogInformation("ProfileViewModel initialized");
        }

        /// <summary>
        /// Gets the main view model for profile management
        /// </summary>
        public MainViewModel ViewModel => _mainViewModel;
    }
}
