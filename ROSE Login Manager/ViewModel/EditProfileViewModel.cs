using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Resources.Util;
using ROSE_Login_Manager.Services;
using System.Windows;
using System.Windows.Input;
using static ROSE_Login_Manager.ViewModel.ProfileActionMessage;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     ViewModel class for editing an existing user profile.
    /// </summary>
    internal class EditProfileViewModel : ProfileViewModelBase
    {
        public ICommand UpdateProfileCommand { get; }



        /// <summary>
        ///     Initializes a new instance of the <see cref="EditProfileViewModel"/> class.
        /// </summary>
        /// <param name="profile">The profile to edit.</param>
        public EditProfileViewModel(UserProfileModel profile)
        {
            ProfileName = profile.ProfileName;
            ProfileEmail = profile.ProfileEmail;
            ExistingEmail = profile.ProfileEmail;

            UpdateProfileCommand = new RelayCommand(UpdateProfile);
        }



        /// <summary>
        ///     Updates the profile and sends appropriate messages based on whether the email is updated.
        /// </summary>
        /// <param name="obj">The window object to close upon successful profile update.</param>
        private void UpdateProfile(object obj)
        {
            if (!ValidProfileData(false))
            {   // Cancel Update Profile event if failed
                return;
            }


            using DatabaseManager db = new();
            using UserProfileModel newProfile = CreateNewProfileModel();

            bool emailUpdated = newProfile.ProfileEmail != ExistingEmail;
            if (emailUpdated)
            {
                WeakReferenceMessenger.Default.Send(new ProfileActionMessage(ExistingEmail, ProfileActionType.Delete));
                WeakReferenceMessenger.Default.Send(new ProfileActionMessage(newProfile, ProfileActionType.Add));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new ProfileActionMessage(newProfile, ProfileActionType.Update));
            }

            if (obj is Window wnd)
            {
                wnd.Close();
            }
        }
    }
}
