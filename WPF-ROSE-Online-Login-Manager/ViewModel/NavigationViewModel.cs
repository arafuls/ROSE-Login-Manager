using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ROSE_Online_Login_Manager.Resources.Util;
using SQLitePCL;

namespace ROSE_Online_Login_Manager.ViewModel
{
    /// <summary>
    ///     View model responsible for managing navigation between different views.
    /// </summary>
    internal class NavigationViewModel : ObservableObject
    {
        #region Accessors
        private object _currentView = new();
        private readonly Dictionary<string, object> _viewCache;



        /// <summary>
        ///     Gets or sets the current view to be displayed in the application.
        /// </summary>
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }
        #endregion



        #region ICommand
        public ICommand HomeCommand { get; set; }
        public ICommand ProfilesCommand { get; set; }
        public ICommand SettingsCommand { get; set; }



        /// <summary>
        ///     Method to navigate to the home view.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void Home(object obj) => NavigateToView<HomeViewModel>();



        /// <summary>
        ///     Method to navigate to the profiles view.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void Profiles(object obj)
        {
            NavigateToView<ProfilesViewModel>();
        }



        /// <summary>
        ///     Method to navigate to the settings view.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void Settings(object obj)
        {
            NavigateToView<SettingsViewModel>();
        }
        #endregion



        /// <summary>
        ///     Constructor for NavigationViewModel.
        /// </summary>
        public NavigationViewModel()
        {
            _viewCache = [];

            // Initialize SQLitePCL.raw and the database manager
            Batteries.Init();
            DatabaseManager db = new();

            // Initialize Relay Commands
            HomeCommand = new RelayCommand(Home);
            ProfilesCommand = new RelayCommand(Profiles);
            SettingsCommand = new RelayCommand(Settings);

            // Set the initial view to the home view
            NavigateToView<HomeViewModel>();
        }



        /// <summary>
        ///     Navigate to the specified view model type.
        /// </summary>
        private void NavigateToView<T>() where T : class
        {
            var typeName = typeof(T).Name;
            if (!_viewCache.TryGetValue(typeName, out object? value))
            {
                value = Activator.CreateInstance<T>();
                _viewCache[typeName] = value;
            }

            CurrentView = value;
        }
    }
}
