using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using ROSE_Online_Login_Manager.Model;
using ROSE_Online_Login_Manager.Resources.Util;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;



namespace ROSE_Online_Login_Manager.ViewModel
{
    internal class SettingsViewModel : ObservableObject
    {
        private readonly GlobalVariables _globalVariables;

        private string? _roseGameFolderPath;
        public string? RoseGameFolderPath
        {
            get { return _roseGameFolderPath; }
            set { SetProperty(ref _roseGameFolderPath, value); }
        }


        #region Commands
        public ICommand GameFolderSearchCommand { get; private set; }



        /// <summary>
        ///     Opens a dialog to select the game folder and updates the global variable with the selected folder path.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void GameFolderSearch(object obj)
        {
            OpenFolderDialog openFolderDialog = new()
            {
                Title = "Select ROSE Online Game Folder",
                ValidateNames = false
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                GlobalVariables.Instance.RoseGameFolder = openFolderDialog.FolderName;
            }
        }
        #endregion



        /// <summary>
        ///     Default Constructor
        /// </summary>
        public SettingsViewModel()
        {
            _globalVariables = GlobalVariables.Instance;
            _globalVariables.PropertyChanged += OnGlobalVariablesPropertyChanged;

            // Initialize ICommand Relays
            GameFolderSearchCommand = new RelayCommand(GameFolderSearch);
        }



        /// <summary>
        ///     Handles the PropertyChanged event of the GlobalVariables class.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnGlobalVariablesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {   
            // Check if the RoseGameFolder property of the GlobalVariables instance has changed
            if (e.PropertyName == nameof(GlobalVariables.Instance.RoseGameFolder))
            {   
                // Truncate the RoseGameFolder path to fit within the maximum length and
                // update the RoseGameFolderPath property for the TextBox display
                RoseGameFolderPath = TruncatePath(GlobalVariables.Instance.RoseGameFolder);
            }
        }



        /// <summary>
        ///     Truncates a file path to fit within a specified maximum length, preserving the folder structure.
        /// </summary>
        /// <param name="path">The file path to truncate.</param>
        /// <returns>The truncated file path.</returns>
        private static string TruncatePath(string path)
        {
            const int MAX_LENGTH = 45;

            // If the path length is already within the limit, return the original path
            if (path.Length <= MAX_LENGTH)
            {
                return path;
            }

            // Split the path into individual folders and reverse the order
            string[] folders = path.Split(Path.DirectorySeparatorChar);
            string[] reversed = folders.Reverse().ToArray();

            // Initialize variables
            string truncatedPath = string.Empty;
            StringBuilder sb = new();

            foreach (var folder in reversed)
            {
                // Ignore the drive prefix when checking length
                if (folder == Path.GetPathRoot(path)) { continue; }

                sb.Clear();

                // Check if adding the current folder exceeds the maximum length
                if ((Path.GetPathRoot(path).Length + truncatedPath.Length + folder.Length + 1) < MAX_LENGTH)
                {
                    // Append the folder to the truncated path
                    sb.Insert(0, folder + "\\");
                    truncatedPath = sb.ToString() + truncatedPath;
                }
                else
                {
                    // If adding the folder exceeds the limit, insert ellipsis and break out of the loop
                    sb.Insert(0, "...\\");
                    truncatedPath = sb.ToString() + truncatedPath;
                    break;
                }
            }

            // Prepend the drive prefix to the truncated path
            truncatedPath = Path.GetPathRoot(path) + truncatedPath;

            return truncatedPath;
        }
    }
}
