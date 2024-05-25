using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using System.Windows;
using System.Windows.Input;
using static ROSE_Login_Manager.ViewModel.ProfileActionMessage;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     View model for adding a new user profile.
    /// </summary>
    internal class AddProfileViewModel : ProfileViewModelBase
    {
        public ICommand AddProfileCommand { get; }
        public ICommand CreateProfileCommand { get; }



        /// <summary>
        ///     Default constructor
        /// </summary>
        public AddProfileViewModel()
        {
            AddProfileCommand = new RelayCommand(AddProfile);
            CreateProfileCommand = new RelayCommand(CreateProfile);
        }



        /// <summary>
        ///     Attempts to add a new user profile.
        /// </summary>
        /// <param name="obj">The window object to close upon successful profile addition.</param>
        private void AddProfile(object obj)
        {
            if (!CreateNewProfile())
            {   // Cancel Add Profile event if failed
                return;
            }

            if (obj is Window wnd)
            {   
                wnd.Close();
            }
        }



        /// <summary>
        ///     Creates a new user profile and resets the input fields.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void CreateProfile(object obj)
        {
            if (CreateNewProfile())
            {
                ResetNewProfileFields();
            }
        }



        /// <summary>
        ///     Creates a new user profile and sends a message to indicate profile addition.
        /// </summary>
        /// <returns>True if the profile creation is successful; otherwise, false.</returns>
        private bool CreateNewProfile()
        {
            if (!ValidProfileData(true))
                return false;

            using UserProfileModel newProfile = CreateNewProfileModel();
            WeakReferenceMessenger.Default.Send(new ProfileActionMessage(newProfile, ProfileActionType.Add));
            return true;
        }
    }
}
