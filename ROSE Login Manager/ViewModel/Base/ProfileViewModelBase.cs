using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Resources.Util;
using ROSE_Login_Manager.Services;
using System.Security;
using System.Security.Cryptography;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     Message class representing an action related to a user profile.
    /// </summary>
    public class ProfileActionMessage
    {
        public enum ProfileActionType { Add, Update, Delete }

        public UserProfileModel Profile { get; set; }
        public ProfileActionType ActionType { get; set; }
        public string Email { get; set; } // Add this property for Delete action

        public ProfileActionMessage(UserProfileModel profile, ProfileActionType actionType)
        {
            Profile = profile;
            ActionType = actionType;
        }

        public ProfileActionMessage(string email, ProfileActionType actionType)
        {
            Email = email;
            ActionType = actionType;
        }
    }



    /// <summary>
    ///     Message class representing an action related resetting the User Password Field.
    /// </summary>
    public class ResetPasswordFieldMessage { }



    /// <summary>
    ///     Base view model class for profile-related functionalities.
    /// </summary>
    internal abstract class ProfileViewModelBase : ObservableObject
    {
        #region Accessors
        private string _profileName = string.Empty;
        public string ProfileName
        {
            get => _profileName;
            set => SetProperty(ref _profileName, value);
        }

        private string _profileEmail = string.Empty;
        public string ProfileEmail
        {
            get => _profileEmail;
            set => SetProperty(ref _profileEmail, value);
        }

        private string _existingEmail = string.Empty;
        public string ExistingEmail
        {
            get => _existingEmail;
            set => SetProperty(ref _existingEmail, value);
        }

        private SecureString _profilePassword = new();
        public SecureString ProfilePassword
        {
            get => _profilePassword;
            set => SetProperty(ref _profilePassword, value);
        }

        private bool _allowProfileAttempt;
        public bool AllowProfileAttempt
        {
            get => _allowProfileAttempt;
            set => SetProperty(ref _allowProfileAttempt, value);
        }
        #endregion



        /// <summary>
        ///     Validates the profile data.
        /// </summary>
        /// <param name="isNewProfile">Flag indicating whether the profile is new.</param>
        /// <returns>True if the profile data is valid; otherwise, false.</returns>
        protected bool ValidProfileData(bool isNewProfile)
        {
            if (string.IsNullOrEmpty(ProfileName) || string.IsNullOrEmpty(ProfileEmail) || ProfilePassword == null || ProfilePassword.Length < 8)
            {
                LogManager.GetCurrentClassLogger().Error("Verify all data fields have been completed and password has 8 or more characters.");
                return false;
            }

            using DatabaseManager db = new();

            // For new profiles, check for email collision directly
            if (isNewProfile && db.PotentialRecordCollision(ProfileEmail))
            {
                LogManager.GetCurrentClassLogger().Error("This profile email is already in use. Each profile must have a unique email address.");
                return false;
            }

            // For updating profiles, check for email collision only if the email has changed
            if (!isNewProfile && ProfileEmail != ExistingEmail && db.PotentialRecordCollision(ProfileEmail))
            {
                LogManager.GetCurrentClassLogger().Error("This profile email is already in use. Each profile must have a unique email address.");
                return false;
            }

            return true;
        }



        /// <summary>
        ///     Creates a new UserProfileModel object with an encrypted password and AES IV.
        /// </summary>
        /// <returns>The new UserProfileModel object.</returns>
        public UserProfileModel CreateNewProfileModel()
        {
            // Generate default AES Initialization Vector to use with encryption
            byte[] iv;
            using (Aes aes = Aes.Create()) { iv = aes.IV; }

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

            return newProfileModel;
        }



        /// <summary>
        ///     Resets the fields for a new profile.
        /// </summary>
        protected void ResetNewProfileFields()
        {
            ProfileName = string.Empty;
            ProfileEmail = string.Empty;
            WeakReferenceMessenger.Default.Send(new ResetPasswordFieldMessage());
        }
    }
}
