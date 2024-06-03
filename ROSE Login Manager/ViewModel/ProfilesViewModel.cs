using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Resources.Util;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static ROSE_Login_Manager.ViewModel.ProfileActionMessage;



namespace ROSE_Login_Manager.ViewModel
{
    internal class ProfilesViewModel : ObservableObject
    {
        private readonly DatabaseManager _db;



        #region Accessors
        private ObservableCollection<UserProfileModel> _userProfilesCollection = [];
        public ObservableCollection<UserProfileModel> UserProfilesCollection
        {
            get => _userProfilesCollection;
            set => SetProperty(ref _userProfilesCollection, value);

        }

        private UserProfileModel? _selectedProfile;
        public UserProfileModel? SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value);

        }
        #endregion



        #region Commands
        public ICommand AddProfileCommand { get; set; }
        public ICommand EditProfileCommand { get; set; }
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
        ///     Method to edit an existing user profile.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void EditProfile(object obj)
        {
            // Open the Add Profile dialog
            EditProfileViewModel editProfileViewModel = new(SelectedProfile);
            EditProfile editProfileView = new()
            {
                DataContext = editProfileViewModel,
                Owner = Application.Current.MainWindow
            };
            editProfileView.ShowDialog();
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
                OnProfileDelete(SelectedProfile.ProfileEmail);
            }
        }
        #endregion



        /// <summary>
        ///     Initializes a new instance of the <see cref="ProfilesViewModel"/> class.
        /// </summary>
        public ProfilesViewModel()
        {
            WeakReferenceMessenger.Default.Register<ProfileActionMessage>(this, OnProfileActionReceived);
            WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, OnDatabaseChangedReceived);

            // Initialize the database manager
            _db = new DatabaseManager();

            // Initialize ICommand Relays
            AddProfileCommand = new RelayCommand(AddProfile);
            EditProfileCommand = new RelayCommand(EditProfile);
            DeleteProfileCommand = new RelayCommand(DeleteProfile);

            LoadAllProfilesFromDatabase();
        }



        /// <summary>
        ///     Handles the event when a database change message is received.
        /// </summary>
        /// <param name="recipient">The recipient of the message.</param>
        /// <param name="message">The database changed message.</param>
        private void OnDatabaseChangedReceived(object recipient, DatabaseChangedMessage message)
        {
            LoadAllProfilesFromDatabase();
        }



        /// <summary>
        ///     Handles the event when a profile action message is received.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="message">The profile action message.</param>
        private void OnProfileActionReceived(object obj, ProfileActionMessage message)
        {
            switch (message.ActionType)
            {
                case ProfileActionType.Add:
                    OnProfileAdd(message.Profile);
                    break;

                case ProfileActionType.Update:
                    OnProfileUpdate(message.Profile);
                    break;

                case ProfileActionType.Delete:
                    OnProfileDelete(message.Email);
                    break;
            }
        }



        /// <summary>
        ///     Handles the addition of a user profile.
        /// </summary>
        /// <param name="profile">The profile to be added.</param>
        private void OnProfileAdd(UserProfileModel profile)
        {
            if (profile == null)
                return;

            _db.AddProfile(profile);
        }



        /// <summary>
        ///     Handles the update of a user profile.
        /// </summary>
        /// <param name="profile">The profile to be updated.</param>
        private void OnProfileUpdate(UserProfileModel profile)
        {
            if (profile == null)
                return;

            _db.UpdateProfile(profile);
        }



        /// <summary>
        ///     Handles the deletion of a user profile.
        /// </summary>
        /// <param name="email">The email of the profile to be deleted.</param>
        private void OnProfileDelete(string email)
        {
            if (string.IsNullOrEmpty(email))
                return;

            _db.DeleteProfile(email);
        }



        /// <summary>
        ///     Loads all profiles from the database.
        /// </summary>
        private void LoadAllProfilesFromDatabase()
        {
            UserProfilesCollection = _db.GetAllProfiles();
        }
    }
}
