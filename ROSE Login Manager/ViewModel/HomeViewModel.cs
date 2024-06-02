using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Resources.Util;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     ViewModel class for the home view, responsible for managing user profiles and launching the ROSE Online client.
    /// </summary>
    internal class HomeViewModel : ObservableObject
    {
        private readonly DatabaseManager _db;



        #region Accessors
        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string _currentFileName;
        public string CurrentFileName
        {
            get => _currentFileName;
            set => SetProperty(ref _currentFileName, value);
        }

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

            WeakReferenceMessenger.Default.Register<LaunchProfileMessage>(this, LaunchProfile);
            WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, OnDatabaseChangedReceived);
            WeakReferenceMessenger.Default.Register<ProgressMessage>(this, OnProgressMessageReceived);

            LoadProfileData();

            // ROSE Updater
            _ = new RoseUpdater();
        }



        #region Message Handlers
        /// <summary>
        ///     Handles the reception of the database change message.
        /// </summary>
        /// <param name="recipient">The recipient of the message.</param>
        /// <param name="message">The received message.</param>
        private void OnDatabaseChangedReceived(object recipient, DatabaseChangedMessage message)
        {
            LoadProfileData();
        }



        /// <summary>
        ///     Handles the launch profile message by starting a new thread to launch the ROSE Online client.
        /// </summary>
        /// <param name="obj">The object parameter.</param>
        /// <param name="message">The launch profile message.</param>
        public void LaunchProfile(object obj, LaunchProfileMessage message)
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
            UserProfileModel? profile = Profiles.FirstOrDefault(p => p.ProfileEmail == message.ProfileEmail);

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



        private void OnProgressMessageReceived(object recipient, ProgressMessage message)
        {
            _ = UpdateProgressAsync(message.ProgressPercentage, message.CurrentFileName);
        }
        #endregion



        private async Task UpdateProgressAsync(int targetProgress, string currentFileName)
        {
            int currentProgress = Progress;

            while (currentProgress < targetProgress)
            {
                currentProgress++;
                Progress = currentProgress;
                CurrentFileName = Progress != 100 ? ("Downloading " + currentFileName) : "Latest";

                // Lower value for smoother and faster animation (e.g., 10 ms)
                // Higher value for slower and potentially less smooth animation (e.g., 50 ms)
                await Task.Delay(1);
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

            if (string.IsNullOrEmpty(startInfo.FileName))
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - HomeViewModel::LoginThread",
                    message: "TRose.exe could not be found.\n\n" +
                             "Confirm that the ROSE Online Folder Location is set correctly.",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
                return;
            }

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
