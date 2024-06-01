using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ROSE_Login_Manager.Services;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     View model responsible for managing navigation between different views.
    /// </summary>
    internal class NavigationViewModel : ObservableObject
    {
        private readonly Dictionary<string, object> _viewCache;



        /// <summary>
        ///     Gets or sets the current view to be displayed in the application.
        /// </summary>
        private object _currentView = new();
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }



        /// <summary>
        ///     Gets or sets a value indicating whether the "Home" button is checked.
        /// </summary>
        private bool _isHomeChecked = true;
        public bool IsHomeChecked
        {
            get => _isHomeChecked;
            set
            {
                if (_isHomeChecked != value)
                {
                    _isHomeChecked = value;
                    if (value)
                    {
                        IsProfilesChecked = false;
                        IsSettingsChecked = false;
                    }
                    OnPropertyChanged(nameof(IsHomeChecked));
                }
            }
        }



        /// <summary>
        ///     Gets or sets a value indicating whether the "Profiles" button is checked.
        /// </summary>
        private bool _isProfilesChecked;
        public bool IsProfilesChecked
        {
            get => _isProfilesChecked;
            set
            {
                if (_isProfilesChecked != value)
                {
                    _isProfilesChecked = value;
                    if (value)
                    {
                        IsHomeChecked = false;
                        IsSettingsChecked = false;
                    }
                    OnPropertyChanged(nameof(IsProfilesChecked));
                }
            }
        }



        /// <summary>
        ///     Gets or sets a value indicating whether the "Settings" button is checked.
        /// </summary>
        private bool _isSettingsChecked;
        public bool IsSettingsChecked
        {
            get => _isSettingsChecked;
            set
            {
                if (_isSettingsChecked != value)
                {
                    _isSettingsChecked = value;
                    if (value)
                    {
                        IsHomeChecked = false;
                        IsProfilesChecked = false;
                    }
                    OnPropertyChanged(nameof(IsSettingsChecked));
                }
            }
        }



        public ICommand HomeCommand { get; }
        private void Home(object obj) => NavigateToView<HomeViewModel>();



        public ICommand ProfilesCommand { get; }
        private void Profiles(object obj) => NavigateToView<ProfilesViewModel>();



        public ICommand SettingsCommand { get; }
        private void Settings(object obj) => NavigateToView<SettingsViewModel>();



        /// <summary>
        ///     Constructor for NavigationViewModel.
        /// </summary>
        public NavigationViewModel()
        {
            _viewCache = [];

            HomeCommand = new RelayCommand(Home);
            ProfilesCommand = new RelayCommand(Profiles);
            SettingsCommand = new RelayCommand(Settings);

            NavigateToView<HomeViewModel>();
        }



        /// <summary>
        ///     Navigates to the view of the specified type and initializes it if not cached.
        /// </summary>
        /// <typeparam name="T">The type of the view model representing the view.</typeparam>
        private void NavigateToView<T>() where T : class
        {
            // Get the name of the specified type and check if the view is already cached
            var typeName = typeof(T).Name;
            if (!_viewCache.TryGetValue(typeName, out object? value))
            {
                value = Activator.CreateInstance<T>();
                _viewCache[typeName] = value;
            }

            // Update the checked state of the corresponding navigation button
            CurrentView = value;
            switch (typeName)
            {
                case nameof(HomeViewModel):
                    IsHomeChecked = true;
                    break;
                case nameof(ProfilesViewModel):
                    IsProfilesChecked = true;
                    break;
                case nameof(SettingsViewModel):
                    IsSettingsChecked = true;
                    break;
            }
        }
    }
}
