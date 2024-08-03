using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using System.Windows.Input;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     View model responsible for managing navigation between different views.
    /// </summary>
    internal class NavigationViewModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, object> _viewCache;
        private readonly Dictionary<string, bool> _buttonStates = new()
        {
            { nameof(IsHomeChecked), true },
            { nameof(IsProfilesChecked), false },
            { nameof(IsAboutMeChecked), false },
            { nameof(IsEventLogChecked), false },
            { nameof(IsSettingsChecked), false }
        };



        #region Accessors
        /// <summary>
        ///     Gets or sets the current view to be displayed in the application.
        /// </summary>
        private object _currentView = new();
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }



        public bool IsHomeChecked
        {
            get => _buttonStates[nameof(IsHomeChecked)];
            set => SetCheckedState(nameof(IsHomeChecked), value);
        }

        public bool IsProfilesChecked
        {
            get => _buttonStates[nameof(IsProfilesChecked)];
            set => SetCheckedState(nameof(IsProfilesChecked), value);
        }

        public bool IsAboutMeChecked
        {
            get => _buttonStates[nameof(IsAboutMeChecked)];
            set => SetCheckedState(nameof(IsAboutMeChecked), value);
        }

        public bool IsEventLogChecked
        {
            get => _buttonStates[nameof(IsEventLogChecked)];
            set => SetCheckedState(nameof(IsEventLogChecked), value);
        }

        public bool IsSettingsChecked
        {
            get => _buttonStates[nameof(IsSettingsChecked)];
            set => SetCheckedState(nameof(IsSettingsChecked), value);
        }
        #endregion Accessors



        #region ICommand
        public ICommand HomeCommand { get; }
        private void Home(object obj) => NavigateToView<HomeViewModel>();



        public ICommand ProfilesCommand { get; }
        private void Profiles(object obj) => NavigateToView<ProfilesViewModel>();



        public ICommand AboutMeCommand { get; }
        private void AboutMe(object obj) => NavigateToView<AboutMeViewModel>();



        public ICommand EventLogCommand { get; }
        private void EventLog(object obj) => NavigateToView<EventLogViewModel>();



        public ICommand SettingsCommand { get; }
        private void Settings(object obj) => NavigateToView<SettingsViewModel>();
        #endregion



        /// <summary>
        ///     Constructor for NavigationViewModel.
        /// </summary>
        public NavigationViewModel()
        {
            _viewCache = [];

            // Initialize all view models
            var viewModels = new[] {
                typeof(HomeViewModel),
                typeof(ProfilesViewModel),
                typeof(AboutMeViewModel),
                typeof(EventLogViewModel),
                typeof(SettingsViewModel)
            };

            foreach (var viewModelType in viewModels)
            {
                var typeName = viewModelType.Name;

                // Instantiate and handle possible null values
                if (Activator.CreateInstance(viewModelType) is object viewModelInstance)
                {
                    _viewCache[typeName] = viewModelInstance;
                }
                else
                {
                    Logger.Fatal($"Failed to create an instance of {viewModelType.FullName}");
                    throw new InvalidOperationException($"Failed to create an instance of {viewModelType.FullName}");
                }
            }

            HomeCommand = new RelayCommand(Home);
            ProfilesCommand = new RelayCommand(Profiles);
            AboutMeCommand = new RelayCommand(AboutMe);
            EventLogCommand = new RelayCommand(EventLog);
            SettingsCommand = new RelayCommand(Settings);

            NavigateToView<HomeViewModel>();
        }



        /// <summary>
        ///     Navigates to the view of the specified type and initializes it if not cached.
        /// </summary>
        /// <typeparam name="T">The type of the view model representing the view.</typeparam>
        private void NavigateToView<T>() where T : class
        {
            var typeName = typeof(T).Name;

            // Retrieve the cached view model
            if (!_viewCache.TryGetValue(typeName, out var value))
            {
                value = Activator.CreateInstance<T>();
                _viewCache[typeName] = value;
            }

            // Update the current view
            CurrentView = value;
            SetCheckedState($"Is{typeName}Checked", true);

            WeakReferenceMessenger.Default.Send(new ViewChangedMessage(typeName));
        }




        /// <summary>
        ///     Updates the checked state of a button. If the key corresponds to a ViewModel,
        ///     it is transformed to the corresponding View key before updating.
        /// </summary>
        /// <param name="key">The key representing the button state.</param>
        /// <param name="value">The new checked state value.</param>
        private void SetCheckedState(string key, bool value)
        {
            if (key.EndsWith("ViewModel"))
            {
                key = string.Concat("Is", key.AsSpan(0, key.Length - "ViewModel".Length), "Checked");
            }

            if (!_buttonStates.TryGetValue(key, out var currentValue) || currentValue == value)
            {
                return;
            }

            // When setting a button to checked, uncheck all other buttons
            if (value)
            {
                foreach (var k in _buttonStates.Keys.ToList())
                {
                    _buttonStates[k] = false;
                }
            }

            // Update the state of the selected button
            _buttonStates[key] = value;
            OnPropertyChanged(nameof(IsHomeChecked));
            OnPropertyChanged(nameof(IsProfilesChecked));
            OnPropertyChanged(nameof(IsAboutMeChecked));
            OnPropertyChanged(nameof(IsEventLogChecked));
            OnPropertyChanged(nameof(IsSettingsChecked));
        }
    }
}
