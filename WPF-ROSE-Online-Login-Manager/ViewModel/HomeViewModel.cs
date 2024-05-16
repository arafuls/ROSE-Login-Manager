using CommunityToolkit.Mvvm.ComponentModel;
using ROSE_Online_Login_Manager.Model;
using ROSE_Online_Login_Manager.Resources.Util;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;



namespace ROSE_Online_Login_Manager.ViewModel
{
    /// <summary>
    /// ViewModel class for the Home view.
    /// </summary>
    internal class HomeViewModel : ObservableObject
    {
        private readonly DatabaseManager db;



        #region Accessors
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
        /// Default Constructor
        /// </summary>
        public HomeViewModel()
        {
            db       = new DatabaseManager();
            Profiles = new ObservableCollection<UserProfileModel>(db.GetAllProfiles());
        }



        public void LaunchProfile(string email)
        {
            var profile = Profiles.FirstOrDefault(p => p.ProfileEmail == email);

            if (profile != null)
            {
                Thread thread = new(() => LoginThread(
                    profile.ProfileEmail,
                    profile.ProfilePassword
                ));
                thread.Start();
            }
        }



        private static void LoginThread(string email, string password)
        {
            if (GlobalVariables.RoseDir == null || GlobalVariables.RoseDir == string.Empty)
            {
                MessageBox.Show("Set you must set ROSE Online directory in the Settings tab.");
                return;
            }

            string exePath = GlobalVariables.RoseDir + "TRose.exe";
            string arguments = $"--login --server connect.roseonlinegame.com --username {email} --password {password}";

            ProcessStartInfo startInfo = new(exePath)
            {
                Arguments = arguments,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
