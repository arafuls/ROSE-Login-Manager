using CommunityToolkit.Mvvm.ComponentModel;
using ROSE_Online_Login_Manager.Model;
using ROSE_Online_Login_Manager.Resources.Util;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Principal;
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
            if (GlobalVariables.RoseDir == null || GlobalVariables.RoseDir == string.Empty)
            {
                // Display an error message if the ROSE Online directory is not set
                _dialogService.ShowMessageBox(
                    message: "You must set the ROSE Online game directory in the Settings tab in order to launch.",
                    title:   "ROSE Online Login Manager",
                    button:  MessageBoxButton.OK,
                    icon:    MessageBoxImage.Error);
                return;
            }

            // Find the user profile with the specified email
            var profile = Profiles.FirstOrDefault(p => p.ProfileEmail == email);

            if (profile != null)
            {   // Start a new thread to handle launching the ROSE Online client with the user's credentials
                Thread thread = new(() => LoginThread(
                    profile.ProfileEmail,
                    profile.ProfilePassword
                ));
                thread.Start();
            }
        }



        /// <summary>
        ///     Thread method to launch the ROSE Online client with the specified email and password.
        /// </summary>
        /// <param name="email">The email associated with the user profile.</param>
        /// <param name="password">The password associated with the user profile.</param>
        private static void LoginThread(string email, string password)
        {
            string exePath = GlobalVariables.RoseDir + "TRose.exe";
            string arguments = $"--login --server connect.roseonlinegame.com --username {email} --password {password}";

            // TODO: Decrypt password here for use

            ProcessStartInfo startInfo = new(exePath)
            {
                Arguments = arguments,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
            };

            try
            {   // Start the ROSE Online client process
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {   // Display an error message if an exception occurs during process start
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
