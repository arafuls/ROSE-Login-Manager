using System.ComponentModel;
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



        /// <summary>
        ///     Searches for the specified file in common locations, including the current directory and standard program folders.
        /// </summary>
        /// <param name="fileName">The name of the file to search for.</param>
        /// <returns>
        ///     The full path to the file if found; otherwise, <see langword="null"/>.
        /// </returns>
        public string FindFile(string fileName)
        {
            // Search for the file in common locations
            string[] paths =
            {
                Environment.CurrentDirectory,
                @"C:\Program Files\",
                @"C:\Program Files (x86)\",
            };

            if (!string.IsNullOrEmpty(RoseGameFolder))
            {
                paths = paths.Append(RoseGameFolder).ToArray();
            }


            foreach (string path in paths)
            {
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    RoseGameFolder ??= Path.GetDirectoryName(fullPath);
                    return fullPath; // Return full path if file is found
                }
            }

            return null; // Return null if file is not found
        }
    }
}
