using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
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
        ///     Checks if the specified directory contains the file "TRose.exe".
        /// </summary>
        /// <param name="directoryPath">The path of the directory to search.</param>
        /// <returns>True if "TRose.exe" is found in the directory, otherwise false.</returns>
        public bool ContainsRoseExec(string? directoryPath = null)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = RoseGameFolder;
            }

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
        ///     Retrieves the install location of a specified application from the Windows Registry.
        /// </summary>
        /// <returns>The install location if found; otherwise, null.</returns>
        public static string GetInstallLocationFromRegistry()
        {
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{975CAD98-4A32-4E44-8681-29A2C4BE0B93}_is1";
            string valueName = "InstallLocation";
            return RegistryRead(registryPath, valueName);
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
        ///     Reads a value from the Windows Registry.
        /// </summary>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="valueName">The name of the value to read.</param>
        /// <returns>The value of the specified registry key; otherwise, null.</returns>
        private static string RegistryRead(string path, string valueName)
        {
            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(path);
                if (key != null)
                {
                    object? value = key.GetValue(valueName);
                    if (value != null)
                    {
                        return value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry: {ex.Message}");
            }

            return null;
        }
    }
}
