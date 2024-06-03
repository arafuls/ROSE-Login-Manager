using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using System.Windows.Input;



namespace ROSE_Login_Manager.ViewModel
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
                ConfigurationManager.Instance.SaveConfigSetting("RoseGameFolder", value);
                IsPathValidImage = GlobalVariables.Instance.ContainsRoseExec(value);
                WeakReferenceMessenger.Default.Send(new GameFolderChanged());
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
                ConfigurationManager.Instance.SaveConfigSetting("MaskEmail", value);

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
            _isPathValidImage = GlobalVariables.Instance.ContainsRoseExec(RoseGameFolderPath);

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
                RoseGameFolderPath = openFolderDialog.FolderName;
            }
        }
    }
}
