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
    /// <summary>
    ///     ViewModel for managing settings related to the application.
    /// </summary>
    internal class SettingsViewModel : ObservableObject
    {
        private readonly GlobalVariables _globalVariables;



        /// <summary>
        ///     Gets or sets the directory path of the ROSE Online game folder.
        /// </summary>
        private string? _roseGameFolderPath;
        public string? RoseGameFolderPath
        {
            get { return _roseGameFolderPath; }
            set
            {
                _roseGameFolderPath = value;

                if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
                {
                    OnPropertyChanged(nameof(RoseGameFolderPath));
                    GlobalVariables.Instance.RoseGameFolder = value;
                    IsPathValidImage = true;
                }
                else
                {
                    IsPathValidImage = false;
                }
            }
        }



        /// <summary>
        ///     Gets or sets a value indicating whether the path is valid
        /// </summary>
        /// <remarks> The bool returned is used to determine which image to use in the view.</remarks>
        private bool _isPathValidImage;
        public bool IsPathValidImage
        {
            get { return _isPathValidImage; }
            set
            {
                _isPathValidImage = value;
                OnPropertyChanged(nameof(IsPathValidImage));
            }
        }



        public ICommand GameFolderSearchCommand { get; private set; }



        /// <summary>
        ///     Default Constructor
        /// </summary>
        public SettingsViewModel()
        {
            _globalVariables = GlobalVariables.Instance;
            _globalVariables.PropertyChanged += OnGlobalVariablesPropertyChanged;

            // Initialize ICommand Relays
            GameFolderSearchCommand = new RelayCommand(GameFolderSearch);

            // Initialize Rose Game Path Textbox Text
            //RoseGameFolderPathDisplay = GlobalVariables.Instance.RoseGameFolder;
            RoseGameFolderPath = GlobalVariables.Instance.RoseGameFolder;
        }



        /// <summary>
        ///     Handles the PropertyChanged event of the GlobalVariables class.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnGlobalVariablesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update RoseGameFolderPath when GlobalVariables.RoseGameFolder changes
            if (e.PropertyName == nameof(GlobalVariables.Instance.RoseGameFolder))
            {
                RoseGameFolderPath = GlobalVariables.Instance.RoseGameFolder;
            }
        }



        /// <summary>
        ///     Opens a dialog to select the game folder and updates the global variable with the selected folder path.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void GameFolderSearch(object obj)
        {
            OpenFolderDialog openFolderDialog = new()
            {
                Title = "Select ROSE Online Game Folder",
                ValidateNames = false,
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                GlobalVariables.Instance.RoseGameFolder = openFolderDialog.FolderName;
            }
        }



        /// <summary>
        ///     Truncates a file path to fit within a specified maximum length, preserving the folder structure.
        /// </summary>
        /// <param name="path">The file path to truncate.</param>
        /// <returns>The truncated file path.</returns>
        private static string TruncatePath(string path)
        {
            const int MAX_PATH_LENGTH = 45;
            
            // If path is null or empty, or its length is within the allowed limit, return the original path
            if (string.IsNullOrEmpty(path) || path.Length <= MAX_PATH_LENGTH)
                return path ?? string.Empty;

            // Initialize variables
            string[] folders     = path.Split(Path.DirectorySeparatorChar);
            string[] reversed    = folders.Reverse().ToArray();
            string truncatedPath = string.Empty;
            StringBuilder sb     = new();

            foreach (string folder in reversed)
            {
                sb.Clear(); // Clear the StringBuilder for each iteration

                // Skip the root folder
                if (folder == Path.GetPathRoot(path))
                    continue;

                // Check if adding the folder exceeds the maximum length
                if ((Path.GetPathRoot(path)?.Length ?? 0) + truncatedPath.Length + folder.Length + 1 < MAX_PATH_LENGTH)
                {
                    // Append the folder to the truncated path
                    sb.Insert(0, folder + Path.DirectorySeparatorChar);
                    truncatedPath = sb + truncatedPath;
                }
                else
                {
                    // If adding the folder exceeds the limit, insert ellipsis and break out of the loop
                    sb.Insert(0, "...\\");
                    truncatedPath = sb + truncatedPath;
                    break;
                }
            }

            // Prepend the drive prefix to the truncated path
            truncatedPath = Path.GetPathRoot(path) + truncatedPath;

            return truncatedPath;
        }
    }
}
