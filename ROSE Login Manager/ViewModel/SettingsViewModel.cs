using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Tomlyn;
using Tomlyn.Model;

namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     ViewModel for managing settings related to the application.
    /// </summary>
    internal class SettingsViewModel : ObservableObject
    {
        #region Accessors
        private string? _roseGameFolderPath;
        public string? RoseGameFolderPath
        {
            get => _roseGameFolderPath;
            set
            {
                if (SetProperty(ref _roseGameFolderPath, value))
                {
                    IsPathValidImage = GlobalVariables.Instance.ContainsRoseExec(value);
                    ConfigurationManager.Instance.SaveConfigSetting("RoseGameFolder", value, "GeneralSettings");
                    WeakReferenceMessenger.Default.Send(new GameFolderChanged());
                }
            }
        }

        private bool _isPathValidImage;
        public bool IsPathValidImage
        {
            get { return _isPathValidImage; }
            set
            {
                SetProperty(ref _isPathValidImage, value, nameof(IsPathValidImage));
            }
        }

        private bool _displayEmailChecked;
        public bool DisplayEmailChecked
        {
            get => _displayEmailChecked;
            set
            {
                if (SetProperty(ref _displayEmailChecked, value))
                {
                    ConfigurationManager.Instance.SaveConfigSetting("DisplayEmail", value, "GeneralSettings");

                    if (!value)
                    {
                        MaskEmailChecked = false;
                    }

                    WeakReferenceMessenger.Default.Send(new DisplayEmailCheckedMessage(value));
                }
            }
        }

        private bool _maskEmailChecked;
        public bool MaskEmailChecked
        {
            get => _maskEmailChecked;
            set
            {
                if (SetProperty(ref _maskEmailChecked, value))
                {
                    ConfigurationManager.Instance.SaveConfigSetting("MaskEmail", value, "GeneralSettings");

                    if (value)
                    {
                        DisplayEmailChecked = true;
                    }

                    WeakReferenceMessenger.Default.Send(new MaskEmailCheckedMessage(value));
                }
            }
        }

        private bool _skipPlanetCutscene;
        public bool SkipPlanetCutscene
        {
            get => _skipPlanetCutscene;
            set
            {
                if (SetProperty(ref _skipPlanetCutscene, value))
                {
                    ConfigurationManager.Instance.SaveConfigSetting("SkipPlanetCutscene", value, "GameSettings");
                    WeakReferenceMessenger.Default.Send(new SkipPlanetCutsceneCheckedMessage(value));
                    UpdateTomlFile("game", "skip_planet_cutscene", value);
                }
            }
        }

        public ObservableCollection<string> LoginScreenOptions { get; set; }
        private string? _selectedLoginScreen;
        public string SelectedLoginScreen
        {
            get => _selectedLoginScreen;
            set
            {
                if (SetProperty(ref _selectedLoginScreen, value))
                {
                    ConfigurationManager.Instance.SaveConfigSetting("LoginScreen", value, "GameSettings");
                    UpdateTomlFile("game", "title_map_id", LoginScreenToInt(value));
                }
            }
        }
        #endregion



        public ICommand GameFolderSearchCommand { get; private set; }
        public ICommand FindGameFolderCommand { get; private set; }
        public ICommand DisplayEmailCheckedCommand { get; private set; }
        public ICommand MaskEmailCheckedCommand { get; private set; }



        /// <summary>
        ///     Default Constructor
        /// </summary>
        public SettingsViewModel()
        {
            // Initialize ICommand Relays
            GameFolderSearchCommand = new RelayCommand(GameFolderSearch);
            FindGameFolderCommand = new RelayCommand(FindGameFolder);

            InitializeAccessors();
            InitializeUserControls();
        }



        /// <summary>
        ///     Initializes the settings variables from the global vars.
        /// </summary>
        private void InitializeAccessors()
        {
            _roseGameFolderPath = GlobalVariables.Instance.RoseGameFolder;
            _isPathValidImage = GlobalVariables.Instance.ContainsRoseExec(RoseGameFolderPath);

            _displayEmailChecked = GlobalVariables.Instance.DisplayEmail;
            _maskEmailChecked = GlobalVariables.Instance.MaskEmail;
            _skipPlanetCutscene = GlobalVariables.Instance.SkipPlanetCutscene;
            _selectedLoginScreen = GlobalVariables.Instance.LoginScreen;
        }



        /// <summary>
        ///     Initializes user controls
        /// </summary>
        private void InitializeUserControls()
        {
            LoginScreenOptions =
            [
                "Random",
                "Treehouse",
                "Adventure Plains",
                "Junon Polis"
            ];
            if (string.IsNullOrEmpty(SelectedLoginScreen)) { SelectedLoginScreen = LoginScreenOptions[0]; }
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
                RoseGameFolderPath = openFolderDialog.FolderName;
            }
        }



        /// <summary>
        ///     Finds the game folder path and sets it to the <see cref="RoseGameFolderPath"/> property.
        /// </summary>
        /// <param name="obj">An object parameter (unused).</param>
        private void FindGameFolder(object obj)
        {
            // Retrieve the game folder path from the registry and assign it to the RoseGameFolderPath property.
            RoseGameFolderPath = GlobalVariables.InstallLocationFromRegistry;
        }



        /// <summary>
        ///     Updates a specific key-value pair within a specified section of a TOML file.
        /// </summary>
        /// <param name="section">The section in the TOML file where the key is located.</param>
        /// <param name="key">The key to be updated within the specified section.</param>
        /// <param name="value">The new value to set for the specified key.</param>
        private static void UpdateTomlFile(string section, string key, object value)
        {
            string? filePath = Directory.GetParent(GlobalVariables.Instance.AppPath)?.FullName;
            filePath = Path.Combine(filePath, "Rednim Games", "ROSE Online", "config", "rose.toml");

            if (string.IsNullOrEmpty(filePath))
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - GlobalVariables::UpdateTomlFile",
                    message: "Failed to locate rose.toml",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
                return;
            }

            string tomlContents = File.ReadAllText(filePath);
            TomlTable tomlTable = Toml.ToModel(tomlContents);

            if (tomlTable.TryGetValue(section, out var sectionTableObj) && sectionTableObj is TomlTable sectionTable)
            {
                // Update the value of the specified key
                sectionTable[key] = FormatTomlValue(value);

                // Serialize the updated TOML table back to string
                string updatedTomlContent = Toml.FromModel(tomlTable);

                // Write the updated content back to the file
                File.WriteAllText(filePath, updatedTomlContent);
            }
            else
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - GlobalVariables::UpdateTomlFile",
                    message: $"Failed to update {key} within rose.toml",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
                return;
            }
        }




        /// <summary>
        ///     Formats a value to be used in a TOML file.
        /// </summary>
        /// <param name="value">The value to be formatted.</param>
        /// <returns>The formatted value.</returns>
        private static object FormatTomlValue(object value)
        {
            switch (value)
            {
                case bool:
                    return (bool)value;
                case int:
                    return (int)value;
                case string:
                    return $"'{value}'";
                default:
                    throw new ArgumentException("Unsupported value type.");
            }
        }



        /// <summary>
        ///     Converts a string representing a login screen location to its corresponding integer value.
        /// </summary>
        /// <param name="value">The string representing the login screen location.</param>
        /// <returns>The integer value corresponding to the login screen location.</returns>
        private static int LoginScreenToInt(string value)
        {
            if (value is string location)
            {
                return location switch
                {
                    "Random" => 0,
                    "Treehouse" => 4,
                    "Adventure Plains" => 7,
                    "Junon Polis" => 16,
                    _ => 0,
                };
            }

            return 0;
        }
    }
}
