using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Resources.Util;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using ROSE_Login_Manager.View;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     View model responsible for managing user profiles.
    /// </summary>
    internal class ProfilesViewModel : ObservableObject
    {
        private readonly DatabaseManager _db;



        #region Accessors
        private ObservableCollection<UserProfileModel> _userProfilesCollection = [];
        public ObservableCollection<UserProfileModel> UserProfilesCollection
        {
            get { return _userProfilesCollection; }
            set
            {
                _userProfilesCollection = value;
                OnPropertyChanged(nameof(UserProfilesCollection));
            }
        }



        private UserProfileModel? _selectedProfile;
        public UserProfileModel? SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                _selectedProfile = value;
                OnPropertyChanged(nameof(SelectedProfile));
            }
        }
        #endregion



        #region Commands
        public ICommand AddProfileCommand { get; set; }
        public ICommand DeleteProfileCommand { get; set; }



        /// <summary>
        ///     Method to add a new user profile.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void AddProfile(object obj)
        {
            // Open the Add Profile dialog
            AddProfileViewModel addProfileViewModel = new();
            AddProfile addProfileView = new()
            {
                DataContext = addProfileViewModel,
                Owner = Application.Current.MainWindow
            };
            addProfileView.ShowDialog();
        }



        /// <summary>
        ///     Method to delete the selected user profile.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void DeleteProfile(object obj)
        {
            // Check if a profile is selected for deletion
            if (SelectedProfile != null)
            {
                // Delete the selected profile from the database
                _db.DeleteProfile(SelectedProfile);
                WeakReferenceMessenger.Default.Send(new ProfileDeletedUpdateMessage(SelectedProfile));

                // Reload all profiles from the database
                LoadAllProfilesFromDatabase();
            }
        }
        #endregion



        /// <summary>
        ///     Default constructor.
        /// </summary>
        public ProfilesViewModel()
        {
            // Register to receive ProfileAddedMessage
            WeakReferenceMessenger.Default.Register<ProfilesViewModel, ProfileAddedMessage>(this, OnProfileAdded);

            // Initialize the database manager
            _db = new DatabaseManager();

            // Initialize ICommand Relays
            AddProfileCommand = new RelayCommand(AddProfile);
            DeleteProfileCommand = new RelayCommand(DeleteProfile);

            LoadAllProfilesFromDatabase();
        }



        /// <summary>
        ///     Method called when a new profile is added.
        /// </summary>
        /// <param name="message">The ProfileAddedMessage containing the new profile.</param>
        private void OnProfileAdded(object sender, ProfileAddedMessage message)
        {
            var newProfile = message.NewProfile;
            if (newProfile != null)
            {
                // Insert the new profile into the database and add it to the collection if successful
                if (_db.InsertProfileIntoDatabase(newProfile))
                {
                    UserProfilesCollection.Add(newProfile);
                    WeakReferenceMessenger.Default.Send(new ProfileAddedUpdateMessage(newProfile));
                }
            }
        }



        /// <summary>
        ///     Load all user profiles from the database into the OberserableCollection list.
        /// </summary>
        private void LoadAllProfilesFromDatabase()
        {
            UserProfilesCollection = _db.GetAllProfiles();
        }
    }
}
