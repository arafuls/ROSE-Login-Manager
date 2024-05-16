using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ROSE_Online_Login_Manager.Resources.Util;
using SQLitePCL;

namespace ROSE_Online_Login_Manager.ViewModel
{
    /// <summary>
    /// View model responsible for managing navigation between different views.
    /// </summary>
    internal class NavigationViewModel : ObservableObject
    {
        #region Accessors

        private object _currentView = new();
        private readonly Dictionary<string, object> _viewCache;

        /// <summary>
        /// The currently displayed view.
        /// </summary>
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        #endregion



        #region ICommand

        // ICommand properties for navigation commands

        /// <summary>
        /// Command to navigate to the home view.
        /// </summary>
        public ICommand HomeCommand { get; set; }

        /// <summary>
        /// Command to navigate to the profiles view.
        /// </summary>
        public ICommand ProfilesCommand { get; set; }

        // ICommand methods for navigation commands

        /// <summary>
        /// Method to navigate to the home view.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void Home(object obj) => NavigateToView<HomeViewModel>();

        /// <summary>
        /// Method to navigate to the profiles view.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void Profiles(object obj)
        {
            NavigateToView<ProfilesViewModel>();
        }

        #endregion



        /// <summary>
        /// Constructor for NavigationViewModel.
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

            // Set the initial view to the home view
            NavigateToView<HomeViewModel>();
        }



        /// <summary>
        /// Navigate to the specified view model type.
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
