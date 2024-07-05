using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using GongSolutions.Wpf.DragDrop;
using NLog;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Resources.Util;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     ViewModel class for the home view, responsible for managing user profiles and launching the ROSE Online client.
    /// </summary>
    internal class HomeViewModel : ObservableObject, IDropTarget
    {
        private static readonly DatabaseManager _db = new();
        private readonly RoseUpdater _roseUpdater;



        #region Accessors
        private bool _gameFolderChanged = false;
        public bool GameFolderChanged
        {
            get => _gameFolderChanged;
            set => _gameFolderChanged = value;
        }

        public bool BoolProgress => Progress == 100;

        private int _progress;
        public int Progress
        {
            get => _progress;
            set
            {
                if (SetProperty(ref _progress, value))
                {
                    CurrentFileName = "Verify File Integrity";
                    OnPropertyChanged(nameof(BoolProgress));
                }
            }
        }

        private string _currentFileName = "Verify File Integrity";
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



        public ICommand LaunchGameCommand { get; }



        /// <summary>
        ///     Initializes a new instance of the <see cref="HomeViewModel"/> class.
        /// </summary>
        public HomeViewModel()
        {
            LaunchGameCommand = new RelayCommand(LaunchGame);

            RegisterMessageHandlers();
            LoadProfileData();

            // Run the patcher if we are certain we are in the right location
            _roseUpdater = new RoseUpdater();
            if (GlobalVariables.Instance.ContainsRoseExec())
            {
                _roseUpdater.RunPatcher();
                GameFolderChanged = false;
            };
        }



        /// <summary>
        ///     Registers message handlers for various types of messages using the WeakReferenceMessenger.
        /// </summary>
        private void RegisterMessageHandlers()
        {
            WeakReferenceMessenger.Default.Register<LaunchProfileMessage>(this, LaunchProfile);
            WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, OnDatabaseChangedReceived);
            WeakReferenceMessenger.Default.Register<ProgressMessage>(this, OnProgressMessageReceived);
            WeakReferenceMessenger.Default.Register<ViewChangedMessage>(this, OnViewChangedMessage);
            WeakReferenceMessenger.Default.Register<GameFolderChanged>(this, OnGameFolderChanged);
            WeakReferenceMessenger.Default.Register<ProgressRequestMessage>(this, OnProgressRequestMessage);
        }



        #region Message Handlers
        /// <summary>
        ///     Handles the reception of a view changed message.
        /// </summary>
        /// <param name="recipient">The recipient of the message.</param>
        /// <param name="message">The message indicating that the view has changed.</param>
        private void OnViewChangedMessage(object recipient, ViewChangedMessage message)
        {
            if (message.ViewModelName != nameof(HomeViewModel)) { return; }

            if (!GameFolderChanged) { return; }

            if (GlobalVariables.Instance.ContainsRoseExec())
            {
                _roseUpdater.RunPatcher();
                GameFolderChanged = false;
            }
            WeakReferenceMessenger.Default.Send(new ProgressResponseMessage(Progress));
        }



        /// <summary>
        ///     Handles the reception of a game folder change message.
        /// </summary>
        /// <param name="recipient">The recipient of the message.</param>
        /// <param name="message">The message indicating that the game folder has changed.</param>
        private void OnGameFolderChanged(object recipient, GameFolderChanged message)
        {
            GameFolderChanged = true;
            Progress = 0;
        }



        /// <summary>
        ///     Handles the ProgressRequestMessage by sending a ProgressResponseMessage with the current progress value.
        /// </summary>
        private void OnProgressRequestMessage(object recipient, ProgressRequestMessage message)
        {
            WeakReferenceMessenger.Default.Send(new ProgressResponseMessage(Progress));
        }



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
        ///     Launches the ROSE Online game client.
        /// </summary>
        /// <param name="obj">An optional parameter passed by the command binding (not used in this method).</param>
        private void LaunchGame(object obj)
        {
            if (GlobalVariables.Instance.RoseGameFolder == null ||
                GlobalVariables.Instance.RoseGameFolder == string.Empty)
            {
                LogManager.GetCurrentClassLogger().Error("You must set the ROSE Online game directory in the Settings tab in order to launch.");
                return;
            }

            new Thread(LoginThread).Start();
        }



        /// <summary>
        ///     Handles the launch profile message by starting a new thread to launch the ROSE Online client.
        /// </summary>
        /// <param name="obj">The object parameter.</param>
        /// <param name="message">The launch profile message.</param>
        private void LaunchProfile(object obj, LaunchProfileMessage message)
        {
            if (GlobalVariables.Instance.RoseGameFolder == null ||
                GlobalVariables.Instance.RoseGameFolder == string.Empty)
            {
                LogManager.GetCurrentClassLogger().Error("You must set the ROSE Online game directory in the Settings tab in order to launch.");
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
        ///     Loads user profiles from the database and updates the corresponding observable collections
        ///     for display in the UI.
        /// </summary>
        private void LoadProfileData()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Profiles = new ObservableCollection<UserProfileModel>(_db.GetAllProfiles());
                if (ProfileCards == null)
                {
                    ProfileCards = [];
                }
                else
                {
                    ProfileCards.Clear();
                }

                bool display = GlobalVariables.Instance.DisplayEmail;
                bool mask = GlobalVariables.Instance.MaskEmail;

                IOrderedEnumerable<UserProfileModel> sortedProfiles = Profiles.OrderBy(p => p.ProfileOrder);
                foreach (UserProfileModel profile in sortedProfiles)
                {
                    ProfileCards.Add(new ProfileCardViewModel(profile.ProfileName, profile.ProfileEmail, display, mask));
                }
            });
        }



        /// <summary>
        ///     Handles the reception of a progress message.
        /// </summary>
        /// <param name="recipient">The recipient of the message.</param>
        /// <param name="message">The progress message containing the progress percentage and current file name.</param>
        private void OnProgressMessageReceived(object recipient, ProgressMessage message)
        {
            UpdateProgressAsync(message.ProgressPercentage, message.CurrentFileName);
        }
        #endregion



        /// <summary>
        ///     Updates the progress asynchronously.
        /// </summary>
        /// <param name="targetProgress">The target progress percentage to reach.</param>
        /// <param name="currentFileName">The name of the current file being processed.</param>
        private void UpdateProgressAsync(int targetProgress, string currentFileName)
        {
            int currentProgress = Progress;

            while (currentProgress < targetProgress)
            {
                currentProgress++;
                Progress = currentProgress;
                CurrentFileName = Progress != 100 ? "Downloading " + currentFileName : "Latest";
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
                FileName = Path.Combine(GlobalVariables.Instance.RoseGameFolder, "trose.exe"),
                WorkingDirectory = GlobalVariables.Instance.RoseGameFolder,
                Arguments = arguments,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
            };

            if (string.IsNullOrEmpty(startInfo.FileName))
            {
                LogManager.GetCurrentClassLogger().Error(
                    "TRose.exe could not be found.\n\n" +
                    "Confirm that the ROSE Online Folder Location is set correctly.");
                return;
            }

            if (_db.GetProfileStatusByEmail(email))
            {
                LogManager.GetCurrentClassLogger().Error("TRose.exe is already running an instance with the specified email.");
                return;
            }

            try
            {
                ProcessManager.Instance.LaunchROSE(startInfo);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
            {
                // ERROR_FILE_NOT_FOUND
                LogManager.GetCurrentClassLogger().Error(ex);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
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


        /// <summary>
        ///     Thread method to launch the ROSE Online client.
        /// </summary>
        private static void LoginThread()
        {
            string arguments = $"--login --server connect.roseonlinegame.com";

            ProcessStartInfo startInfo = new()
            {
                FileName = Path.Combine(GlobalVariables.Instance.RoseGameFolder, "trose.exe"),
                WorkingDirectory = GlobalVariables.Instance.RoseGameFolder,
                Arguments = arguments,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
            };

            if (string.IsNullOrEmpty(startInfo.FileName))
            {
                LogManager.GetCurrentClassLogger().Error(
                    "TRose.exe could not be found.\n\n" +
                    "Confirm that the ROSE Online Folder Location is set correctly.");
                return;
            }

            try
            {
                ProcessManager.Instance.LaunchROSE(startInfo);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
            {   // ERROR_FILE_NOT_FOUND
                LogManager.GetCurrentClassLogger().Error(ex);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
            }
        }



        /// <summary>
        ///     Handles the DragOver event for a drag-and-drop operation. This method is invoked when an object is dragged over the target area.
        /// </summary>
        /// <param name="dropInfo">Contains information about the drag-and-drop operation, including the dragged data and the target collection.</param>
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ProfileCardViewModel && dropInfo.TargetCollection == ProfileCards)
            {
                //dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }



        /// <summary>
        ///     Handles the drop operation for profile cards. This method is called when a profile card is dropped
        ///     onto the target collection. It reorders the profile cards based on the drop position.
        /// </summary>
        /// <param name="dropInfo">Information about the drop event, including the data being dropped and the target collection.</param>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ProfileCardViewModel sourceItem && dropInfo.TargetCollection == ProfileCards)
            {
                int sourceIndex = ProfileCards.IndexOf(sourceItem);
                int targetIndex = GetTargetIndexFromDropInfo(dropInfo);

                if (sourceIndex != targetIndex)
                {
                    // Check bounds to prevent OutOfRangeException
                    if (sourceIndex < 0 || sourceIndex >= ProfileCards.Count) return;
                    if (targetIndex < 0 || targetIndex > ProfileCards.Count) return;

                    // Adjust targetIndex to avoid OutOfRangeException
                    if (targetIndex >= ProfileCards.Count) targetIndex = ProfileCards.Count - 1;

                    ProfileCards.Move(sourceIndex, targetIndex);
                    _ = ReorderProfiles();
                }
            }
        }



        /// <summary>
        ///     Determines the target index for the drop operation based on the drop information.
        ///     This method calculates the index where the item should be dropped using the visual tree and mouse position.
        /// </summary>
        /// <param name="dropInfo">Information about the drop event, including the data being dropped and the target collection.</param>
        /// <returns>The target index where the item should be inserted. If no valid index is found, returns the count of items in the ItemsControl.</returns>
        private static int GetTargetIndexFromDropInfo(IDropInfo dropInfo)
        {
            if (dropInfo.VisualTarget is not ItemsControl itemsControl) return -1;

            Point position = dropInfo.DropPosition;
            int index = -1;

            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                FrameworkElement itemContainer = (FrameworkElement)itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (itemContainer != null)
                {
                    Rect bounds = VisualTreeHelper.GetDescendantBounds(itemContainer);
                    Point itemPosition = itemContainer.TransformToAncestor(itemsControl).Transform(new Point(0, 0));
                    bounds.Offset(itemPosition.X, itemPosition.Y);

                    if (bounds.Contains(position))
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index == -1 ? itemsControl.Items.Count : index;
        }



        /// <summary>
        ///     Reorders the user profiles based on the current order of profile cards in the UI.
        /// </summary>
        /// <remarks>
        ///     This method asynchronously updates the ProfileOrder property of each user profile based on the current order of profile cards in the UI.
        ///     It iterates through the ProfileCards collection and updates the ProfileOrder property of each corresponding user profile in the Profiles collection.
        ///     The updated order is then saved to the database asynchronously.
        /// </remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ReorderProfiles()
        {
            List<UserProfileModel> newProfilesOrder = [];

            for (int i = 0; i < ProfileCards.Count; i++)
            {
                ProfileCardViewModel profileCard = ProfileCards[i];

                UserProfileModel? profile = Profiles.FirstOrDefault(p => p.ProfileEmail == profileCard.ProfileEmail);
                if (profile != null)
                {
                    profile.ProfileOrder = i;
                    newProfilesOrder.Add(profile);
                }
            }

            await Task.Run(() => _db.UpdateProfileOrder(Profiles));
        }
    }
}
