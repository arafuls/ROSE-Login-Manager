using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using ROSE_Online_Login_Manager.Model;
using ROSE_Online_Login_Manager.Services;
using ROSE_Online_Login_Manager.Services.Infrastructure;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;



namespace ROSE_Online_Login_Manager.ViewModel
{
    /// <summary>
    ///     ViewModel for managing settings related to the application.
    /// </summary>
    internal class SettingsViewModel : ObservableObject
    {
        #region Accessors
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

                if (Directory.Exists(value))
                {
                    ConfigurationManager.Instance.SaveConfigSetting("RoseGameFolder", value);
                }

                IsPathValidImage = ContainsRoseExec(value);
                OnPropertyChanged(nameof(RoseGameFolderPath));
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



        /// <summary>
        ///     Gets or sets a value indicating whether the DisplayEmailCheckbox is checked.
        /// </summary>
        private bool _displayEmailChecked;
        public bool DisplayEmailChecked
        {
            get => _displayEmailChecked;
            set
            {
                if (_displayEmailChecked == value) { return; }

                SetProperty(ref _displayEmailChecked, value);
                ConfigurationManager.Instance.SaveConfigSetting("DisplayEmail", value);


                if (!value && MaskEmailChecked)
                {
                    MaskEmailChecked = false;
                }

                WeakReferenceMessenger.Default.Send(new DisplayEmailCheckedMessage(value));
            }
        }



        /// <summary>
        ///     Gets or sets a value indicating whether the MaskEmailCheckbox is checked.
        /// </summary>
        private bool _maskEmailChecked;
        public bool MaskEmailChecked
        {
            get => _maskEmailChecked;
            set
            {
                SetProperty(ref _maskEmailChecked, value);
                ConfigurationManager.Instance.SaveConfigSetting("MaskEmail",value);

                if (value && !DisplayEmailChecked)
                {
                    DisplayEmailChecked = true;
                }

                WeakReferenceMessenger.Default.Send(new MaskEmailCheckedMessage(value));
            }
        }
        #endregion



        public ICommand GameFolderSearchCommand { get; private set; }
        public ICommand DisplayEmailCheckedCommand { get; private set; }
        public ICommand MaskEmailCheckedCommand { get; private set; }



        /// <summary>
        ///     Default Constructor
        /// </summary>
        public SettingsViewModel()
        {
            // Initialize ICommand Relays
            GameFolderSearchCommand = new RelayCommand(GameFolderSearch);

            InitializeSettingsVariables();
        }



        /// <summary>
        ///     Initializes the settings variables from the global vars.
        /// </summary>
        private void InitializeSettingsVariables()
        {
            _roseGameFolderPath = GlobalVariables.Instance.RoseGameFolder;
            _displayEmailChecked = GlobalVariables.Instance.DisplayEmail;
            _maskEmailChecked = GlobalVariables.Instance.MaskEmail;
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
                ConfigurationManager.Instance.SaveConfigSetting("RoseGameFolder", openFolderDialog.FolderName);

                // If path changed (is valid dir), then update local var
                if (GlobalVariables.Instance.RoseGameFolder == openFolderDialog.FolderName)
                {
                    RoseGameFolderPath = openFolderDialog.FolderName;
                }
            }
        }



        /// <summary>
        ///     Checks if the specified directory contains the file "TRose.exe".
        /// </summary>
        /// <param name="directoryPath">The path of the directory to search.</param>
        /// <returns>True if "TRose.exe" is found in the directory, otherwise false.</returns>
        private static bool ContainsRoseExec(string directoryPath)
        {
            try
            {
                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    return false;
                }

                // Get all files in the directory
                string[] files = Directory.GetFiles(directoryPath);

                // Check if any file matches "TRose.exe"
                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Equals("TRose.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                // TRose.exe not found in the directory
                return false;
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - SettingsViewModel::ContainsRoseExec",
                    message: ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
                return false;
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
