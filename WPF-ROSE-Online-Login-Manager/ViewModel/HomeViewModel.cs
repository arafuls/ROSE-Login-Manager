using CommunityToolkit.Mvvm.ComponentModel;
using ROSE_Online_Login_Manager.Model;
using ROSE_Online_Login_Manager.Resources.Util;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;



namespace ROSE_Online_Login_Manager.ViewModel
{
    internal class HomeViewModel : ObservableObject
    {
        private readonly DatabaseManager db;
        private readonly IDialogService _dialogService;



        #region Accessors
        /// <summary>
        ///     Gets or sets the collection of user profiles.
        /// </summary>
        private ObservableCollection<UserProfileModel> _profiles;
        public ObservableCollection<UserProfileModel> Profiles
        {
            get { return _profiles; }
            set
            {
                _profiles = value;
                OnPropertyChanged(nameof(Profiles));
            }
        }
        #endregion



        /// <summary>
        ///     Default Constructor
        /// </summary>
        public HomeViewModel()
        {

        }



        /// <summary>
        ///     Initializes a new instance of the <see cref="HomeViewModel"/> class.
        /// </summary>
        /// <param name="dialogService">The dialog service for displaying dialogs.</param>
        public HomeViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            db       = new DatabaseManager();
            Profiles = new ObservableCollection<UserProfileModel>(db.GetAllProfiles());
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
                _dialogService.ShowMessageBox(
                    message: "You must set the ROSE Online game directory in the Settings tab in order to launch.",
                    title:   "ROSE Online Login Manager",
                    button:  MessageBoxButton.OK,
                    icon:    MessageBoxImage.Error);
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
        private void LoginThread(string email, string password, string iv)
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
                _dialogService.ShowMessageBox(
                    title: "ROSE Online Login Manager - File Not Found",
                    message: "The ROSE Online client executable, TRose.exe, could not be found.\n\n" +
                                "Confirm that the ROSE Online client is installed correctly and that the ROSE Online Folder Location is set correctly.",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
            catch (Exception ex)
            {   // Display a generic error message for other exceptions
                _dialogService.ShowMessageBox(
                    title: "ROSE Online Login Manager - An Error Occurred",
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
