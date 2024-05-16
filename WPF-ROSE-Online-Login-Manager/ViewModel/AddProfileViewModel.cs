using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ROSE_Online_Login_Manager.Model;
using ROSE_Online_Login_Manager.Resources.Util;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ROSE_Online_Login_Manager.ViewModel
{
    /// <summary>
    /// Message class to carry the information about a newly added profile.
    /// </summary>
    public class ProfileAddedMessage
    {
        public UserProfileModel? NewProfile { get; set; }
    }



    /// <summary>
    /// View model for adding a new user profile.
    /// </summary>
    internal class AddProfileViewModel : ObservableObject
    {
        #region Accessors
        private string _profileName = string.Empty;
        public string ProfileName
        {
            get { return _profileName; }
            set
            {
                _profileName = value;
                OnPropertyChanged(nameof(ProfileName));
                DetermineButtonState();
            }
        }

        private string _profileEmail = string.Empty;
        public string ProfileEmail
        {
            get { return _profileEmail; }
            set
            {
                _profileEmail = value;
                OnPropertyChanged(nameof(ProfileEmail));
                DetermineButtonState();
            }
        }

        private SecureString _profilePassword = new();
        public SecureString ProfilePassword
        {
            get { return _profilePassword; }
            set
            {
                _profilePassword = value;
                OnPropertyChanged(nameof(ProfilePassword));
                DetermineButtonState();
            }
        }

        private bool _allowAddProfileAttempt;
        public bool AllowAddProfileAttempt
        {
            get { return _allowAddProfileAttempt; }
            set
            {
                _allowAddProfileAttempt = value;
                OnPropertyChanged(nameof(AllowAddProfileAttempt));
            }
        }

        #endregion



        #region ICommand
        /// <summary>
        /// Command to add a new profile.
        /// </summary>
        public ICommand AddProfileCommand { get; }

        /// <summary>
        /// Command to create a new profile.
        /// </summary>
        public ICommand CreateProfileCommand { get; }



        /// <summary>
        ///     Attempts to create a new user profile and closes the window upon successful profile creation.
        /// </summary>
        /// <param name="obj">The object parameter representing the window to close upon successful profile creation.</param>
        private void AddProfile(object obj)
        {
            // Attempt to create profile
            if (!CreateNewProfile())
            {
                // Cancel Add Profile event if failed
                return;
            }

            // Close the window on success
            if (obj is Window wnd)
            {
                wnd.Close();
            }
        }



        /// <summary>
        ///     Initiates the process of creating a new user profile.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void CreateProfile(object obj)
        {
            CreateNewProfile();
        }
        #endregion



        /// <summary>
        ///     Creates a new user profile and sends a message to notify the ProfilesViewModel.
        /// </summary>
        /// <returns>True if the profile creation is successful, otherwise false.</returns>
        private bool CreateNewProfile()
        {
            using DatabaseManager db = new();

            // Perform validation and collision checks
            if (!ValidProfileData(db))
            {
                return false;
            }

            // Notify ProfilesViewModel of the new profile
            NewUserProfileModelMessage(CreateNewProfileModel());

            return true;
        }



        /// <summary>
        ///     Sends a message to the ProfilesViewModel about the new profile, and resets view's data fields.
        /// </summary>
        /// <param name="userProfileModel">The new UserProfileModel to send in the message.</param>
        private void NewUserProfileModelMessage(UserProfileModel userProfileModel)
        {
            // Inform the ProfilesViewModel about the new profile
            var message = new ProfileAddedMessage { NewProfile = userProfileModel };
            WeakReferenceMessenger.Default.Send(message);

            // Reset the view's data field for next profile
            ProfileName = string.Empty;
            ProfileEmail = string.Empty;

            // PasswordBox does not support TwoWay binding so send a message to reset
            WeakReferenceMessenger.Default.Send("ResetPasswordField");
        }



        /// <summary>
        ///     Creates a new UserProfileModel object with an encrypted password and AES IV.
        /// </summary>
        /// <returns>The new UserProfileModel object.</returns>
        private UserProfileModel CreateNewProfileModel()
        {
            // Generate default AES Initialization Vector to use with encryption
            byte[] iv;
            using(Aes aes = Aes.Create()) { iv = aes.IV; }

            // Generate and return the encrypted profile password
            AESEncryptor encryptor = new();
            string encryptedPassword = Convert.ToBase64String(encryptor.Encrypt(ProfilePassword, iv));

            // Create new profile model
            UserProfileModel newProfileModel = new()
            {
                ProfileStatus = false,
                ProfileName = ProfileName,
                ProfileEmail = ProfileEmail,
                ProfilePassword = encryptedPassword,
                ProfileIV = Convert.ToBase64String(iv)
            };
            /*
             
            MessageBox.Show(
                "IV: " + Convert.ToBase64String(iv) + '\n' +
                "Encrytped: " + encryptedPassword);
            
             */
            return newProfileModel;
        }



        /// <summary>
        ///     Validates profile data fields and checks for collisions in the database.
        /// </summary>
        /// <param name="db">The DatabaseManager instance to perform database checks.</param>
        /// <returns>True if the profile data is valid and does not collide with existing records, otherwise false.</returns>
        private bool ValidProfileData(DatabaseManager db)
        {
            // Check for basic valid user credentials
            if (ProfileName == null || ProfileEmail == null || ProfilePassword == null || ProfilePassword.Length < 8)
            {
                string msg = "Unable to create profile.\n\n Verify all data fields have been completed and password has 8 or more characters.";
                string caption = "Error Creating New Profile";
                MessageBox.Show(msg, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Check for collision before continuing
            if (db.PotentialRecordCollision(ProfileEmail))
            {
                string msg = "This profile email is already in use.\n\n Each profile must have a unique email address.";
                string caption = "Duplicate Profile Email";
                MessageBox.Show(msg, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }



        /// <summary>
        ///     Default constructor
        /// </summary>
        public AddProfileViewModel()
        {
            // Initialize ICommand properties
            AddProfileCommand = new RelayCommand(AddProfile);
            CreateProfileCommand = new RelayCommand(CreateProfile);
        }



        /// <summary>
        ///     Determines the state of the add profile button based on the validity of profile data.
        /// </summary>
        public void DetermineButtonState()
        {
            bool nameValid = !string.IsNullOrEmpty(ProfileName);
            bool emailValid = !string.IsNullOrEmpty(ProfileEmail);
            bool passwordValid = ProfilePassword != null && ProfilePassword.Length >= 8;

            AllowAddProfileAttempt = (nameValid && emailValid && passwordValid);
        }

    }
}
