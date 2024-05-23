using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ROSE_Online_Login_Manager.Model;
using ROSE_Online_Login_Manager.Resources.Util;
using ROSE_Online_Login_Manager.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;



namespace ROSE_Online_Login_Manager.ViewModel
{
    internal class HomeViewModel : ObservableObject
    {
        private readonly DatabaseManager _db;



        #region Accessors
        /// <summary>
        ///     Gets or sets the collection of profile card view models.
        /// </summary>
        private ObservableCollection<ProfileCardViewModel> _profileCards;
        public ObservableCollection<ProfileCardViewModel> ProfileCards
        {
            get => _profileCards;
            set
            {
                _profileCards = value;
                OnPropertyChanged(nameof(ProfileCards));
            }
        }



        /// <summary>
        ///     Gets or sets the collection of user profiles.
        /// </summary>
        private ObservableCollection<UserProfileModel> _profiles;
        public ObservableCollection<UserProfileModel> Profiles
        {
            get => _profiles;
            set
            {
                _profiles = value;
                OnPropertyChanged(nameof(Profiles));
            }
        }
        #endregion



        /// <summary>
        ///     Initializes a new instance of the <see cref="HomeViewModel"/> class.
        /// </summary>
        public HomeViewModel()
        {
            _db = new DatabaseManager();

            WeakReferenceMessenger.Default.Register<LaunchProfileMessage>(this, (recipient, message) => LaunchProfile(message.ProfileEmail));

            LoadProfileData();
        }



        /// <summary>
        ///     Loads the user profiles and initializes the corresponding profile card view models.
        /// </summary>
        private void LoadProfileData()
        {
            ProfileCards = [];

            bool display = GlobalVariables.Instance.DisplayEmail;
            bool mask = GlobalVariables.Instance.MaskEmail;

            Profiles = new ObservableCollection<UserProfileModel>(_db.GetAllProfiles());
            foreach (UserProfileModel profile in Profiles)
            {
                ProfileCards.Add(new ProfileCardViewModel(profile.ProfileName, profile.ProfileEmail, display, mask));
            }
        }



        /// <summary>
        ///     Launches the ROSE Online client with the provided user profile credentials.
        /// </summary>
        /// <param name="email">The email associated with the user profile.</param>
        public void LaunchProfile(string email)
        {
            if (GlobalVariables.Instance.RoseGameFolder == null || 
                GlobalVariables.Instance.RoseGameFolder == string.Empty)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - HomeViewModel::LaunchProfile",
                    message: "You must set the ROSE Online game directory in the Settings tab in order to launch.",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
                return;
            }

            // Find the user profile with the specified email
            UserProfileModel? profile = Profiles.FirstOrDefault(p => p.ProfileEmail == email);

            if (profile != null)
            {   // Start a new thread to handle launching the ROSE Online client with the user's credentials
                Thread thread = new(() => LoginThread(
                    profile.ProfileEmail,
                    profile.ProfilePassword,
                    profile.ProfileIV
                ));
                thread.Start();
            }
        }



        /// <summary>
        ///     Thread method to launch the ROSE Online client with the specified email and password.
        /// </summary>
        /// <param name="email">The email associated with the user profile.</param>
        /// <param name="password">The password associated with the user profile.</param>
        /// <param name="iv">The initialization vector associated with the user profile to be used for decryption.</param>
        private static void LoginThread(string email, string password, string iv)
        {
            string decryptedPassword = AESEncryptor.Decrypt(Convert.FromBase64String(password), Convert.FromBase64String(iv));
            string arguments = $"--login --server connect.roseonlinegame.com --username {email} --password {decryptedPassword}";

            ProcessStartInfo startInfo = new()
            {
                FileName = GlobalVariables.Instance.FindFile("TRose.exe"),
                WorkingDirectory = GlobalVariables.Instance.RoseGameFolder,
                Arguments = arguments,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
            };

            try
            {   // Start the ROSE Online client process
                Process.Start(startInfo);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
            {   // ERROR_FILE_NOT_FOUND
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - HomeViewModel::LoginThread",
                    message: "The ROSE Online client executable, TRose.exe, could not be found.\n\n" +
                             "Confirm that the ROSE Online client is installed correctly and that the ROSE Online Folder Location is set correctly.",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
            catch (Exception ex)
            {   // Display a generic error message for other exceptions
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - HomeViewModel::LoginThread",
                    message: ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
            finally
            {
                // Clear sensitive information from memory to prevent exposure
                // Note: These variables contain sensitive data and must be cleared before exiting the method.
                // IDE0059 warning is ignored as the assignments are necessary for security reasons.
#pragma warning disable IDE0059
                decryptedPassword = string.Empty;
                arguments = string.Empty;
#pragma warning restore IDE0059

                startInfo.Arguments = string.Empty;
            }
        }
    }
}