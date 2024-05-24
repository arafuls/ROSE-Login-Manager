using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using System.IO;
using System.Windows;



namespace ROSE_Login_Manager.Model
{
    /// <summary>
    ///     Represents a singleton class for storing global variables or application-wide state.
    /// </summary>
    public class GlobalVariables : ObservableObject
    {
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
                }
            }
        }



        /// <summary>
        ///     Gets or sets a value indicating whether the "Display Email" option is enabled.
        /// </summary>
        private bool _displayEmail;
        public bool DisplayEmail
        {
            get { return _displayEmail; }
            set
            {
                if (_displayEmail != value)
                {
                    _displayEmail = value;
                }
            }
        }



        /// <summary>
        ///     Gets or sets a value indicating whether the "Mask Email" option is enabled.
        /// </summary>
        private bool _maskEmail;
        public bool MaskEmail
        {
            get { return _maskEmail; }
            set
            {
                if (_maskEmail != value)
                {
                    _maskEmail = value;
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
        #endregion



        /// <summary>
        ///     Gets the singleton instance of the GlobalVariables class.
        /// </summary>
        private static readonly Lazy<GlobalVariables> lazyInstance = new(() => new GlobalVariables());
        public static GlobalVariables Instance => lazyInstance.Value;



        /// <summary>
        ///     Private constructor to prevent external instantiation
        /// </summary>
        private GlobalVariables()
        {
            _appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ROSE Online Login Manager");

            WeakReferenceMessenger.Default.Register<SettingChangedMessage<string>>(this, HandleSettingChanged);
            WeakReferenceMessenger.Default.Register<SettingChangedMessage<bool>>(this, HandleSettingChanged);
        }



        /// <summary>
        ///     Handles the change of a setting by updating the corresponding property in the recipient object.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="recipient">The object that will receive the updated setting.</param>
        /// <param name="message">The message containing information about the changed setting.</param>
        private void HandleSettingChanged<T>(object recipient, SettingChangedMessage<T> message)
        {
            // Check if the property exists in GlobalVariables
            System.Reflection.PropertyInfo? property = GetType().GetProperty(message.Key);
            if (property != null)
            {
                // Convert the value to the property's type and set the property value
                object? convertedValue = Convert.ChangeType(message.Value, property.PropertyType);
                property.SetValue(this, convertedValue);
            }
            else
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - GlobalVariables::HandleSettingChanged",
                    message: "Unknown setting changed.",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
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
            [
                Environment.CurrentDirectory,
                @"C:\Program Files\",
                @"C:\Program Files (x86)\",
            ];

            if (!string.IsNullOrEmpty(RoseGameFolder))
            {
                paths = [.. paths, RoseGameFolder];
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
