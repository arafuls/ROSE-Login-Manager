﻿using System.ComponentModel;
using System.IO;



namespace ROSE_Online_Login_Manager.Model
{
    /// <summary>
    ///     Represents a singleton class for storing global variables or application-wide state.
    /// </summary>
    public class GlobalVariables : INotifyPropertyChanged
    {
        public event EventHandler RoseGameFolderChanged;



        #region Accessors
        /// <summary>
        ///     Gets or sets the directory path of the ROSE Online game folder.
        /// </summary>
        private string? _roseGameFolder;
        public string? RoseGameFolder
        {
            get { return _roseGameFolder; }
            set
            {
                if (_roseGameFolder != value)
                {
                    _roseGameFolder = value;
                    OnPropertyChanged(nameof(RoseGameFolder));
                    RoseGameFolderChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }



        /// <summary>
        ///     Gets the application path.
        /// </summary>
        private readonly string _appPath;
        public string AppPath
        {
            get { return _appPath; }
        }



        /// <summary>
        ///     Gets the singleton instance of the GlobalVariables class.
        /// </summary>
        private static GlobalVariables _instance;
        public static GlobalVariables Instance
        {
            get
            {
                _instance ??= new GlobalVariables();    // Create the instance if it doesn't exist yet
                return _instance;
            }
        }
        #endregion



        /// <summary>
        ///     Private constructor to prevent external instantiation
        /// </summary>
        private GlobalVariables()
        {
            _appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ROSE Online Login Manager");
        }



        /// <summary>
        ///     Raises the PropertyChanged event to notify subscribers of a property change.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}