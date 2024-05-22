using ROSE_Online_Login_Manager.Resources.Util;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml;

/* 
 * TODO: 
 *   - Attempt to find RoseGameFolder on start up if not set in config.xml
 */

namespace ROSE_Online_Login_Manager.Model
{
    /// <summary>
    ///     Represents a singleton class responsible for managing and storing configuration settings.
    /// </summary>
    class ConfigurationManager : IDisposable
    {
        private readonly GlobalVariables _globalVariables;
        private readonly string _configFile;
        private readonly XmlDocument _doc;



        private static readonly ConfigurationManager instance = new();



        /// <summary>
        ///     Static constructor to initialize the singleton instance of ConfigurationManager.
        /// </summary>
        static ConfigurationManager() { }



        /// <summary>
        ///     Private constructor to prevent external instantiation and initialize the ConfigurationManager.
        /// </summary>
        private ConfigurationManager()
        {
            _globalVariables = GlobalVariables.Instance;
            _configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ROSE Online Login Manager") + "\\config.xml";
            _doc = new XmlDocument();

            if (File.Exists(_configFile))
            {
                LoadConfig();
            }
            else
            {
                CreateConfig();
            }

            _globalVariables.PropertyChanged += HandleGlobalVariablesChanged;
        }



        /// <summary>
        ///     Gets the singleton instance of ConfigurationManager.
        /// </summary>
        public static ConfigurationManager Instance => instance;



        /// <summary>
        ///     Loads configuration settings from the XML file.
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                _doc.Load(_configFile);

                XmlNode? generalSettingsNode = _doc.SelectSingleNode("//Configuration/GeneralSettings");
                if (generalSettingsNode != null)
                {   
                    _globalVariables.RoseGameFolder = GetConfigSetting("RoseGameFolder");
                    _globalVariables.DisplayEmail   = bool.Parse(GetConfigSetting("DisplayEmail"));
                    _globalVariables.MaskEmail      = bool.Parse(GetConfigSetting("MaskEmail"));
                }
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - ConfigurationManager::LoadConfig",
                    message: "Error loading configuration file: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
        }



        /// <summary>
        ///     Creates a new configuration file with default settings.
        /// </summary>
        private void CreateConfig()
        {
            try
            {
                XmlElement root = _doc.CreateElement("Configuration");
                _doc.AppendChild(root);

                XmlElement generalSettings = _doc.CreateElement("GeneralSettings");
                root.AppendChild(generalSettings);

                SaveConfigSetting("RoseGameFolder", "");
                SaveConfigSetting("DisplayEmail", "False");
                SaveConfigSetting("MaskEmail", "False");

                _doc.Save(_configFile);
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - ConfigurationManager::CreateConfig",
                    message: "Error creating configuration file: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
        }



        /// <summary>
        ///     Handles the event triggered when global variables are changed by saving them.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void HandleGlobalVariablesChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSetting(e.PropertyName, _globalVariables.GetType().GetProperty(e.PropertyName)?.GetValue(_globalVariables, null)?.ToString() ?? "");
        }



        /// <summary>
        ///     Saves a configuration setting with the specified key and value.
        /// </summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        private void SaveSetting(string key, string value)
        {
            try
            {
                XmlNode? root = _doc.SelectSingleNode("//Configuration/GeneralSettings");
                if (root != null)
                {
                    XmlNode? existingSetting = root.SelectSingleNode(key);
                    if (existingSetting != null)
                    {
                        // If the setting exists, update its value.
                        existingSetting.InnerText = value;
                    }
                    else
                    {
                        // If the setting does not exist, create a new setting element.
                        XmlElement element = _doc.CreateElement(key);
                        element.InnerText = value;
                        root.AppendChild(element);
                    }
                    _doc.Save(_configFile);
                }
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - ConfigurationManager::SaveSetting",
                    message: "Error saving setting: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
        }



        /// <summary>
        ///     Saves a configuration setting with the specified key and boolean value.
        /// </summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The boolean value of the setting.</param>
        public void SaveConfigSetting(string key, bool value)
        {
            SaveSetting(key, value.ToString());
        }



        /// <summary>
        ///     Saves a configuration setting with the specified key and string value.
        /// </summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The string value of the setting.</param>
        public void SaveConfigSetting(string key, string value)
        {
            SaveSetting(key, value);
        }



        /// <summary>
        ///     Retrieves the value of a configuration setting specified by the key.
        /// </summary>
        /// <param name="key">The key of the configuration setting to retrieve.</param>
        /// <returns>
        ///     The value of the configuration setting if found; otherwise, returns null.
        /// </returns>
        public string GetConfigSetting(string key)
        {
            XmlNode? root = _doc.SelectSingleNode("//Configuration/GeneralSettings");
            if (root != null)
            {
                XmlNode? settingNode = root.SelectSingleNode(key);
                if (settingNode != null)
                {
                    return settingNode.InnerText;
                }
            }
            return null;
        }



        /// <summary>
        ///     Releases all resources used by the ConfigurationManager.
        /// </summary>
        public void Dispose()
        {
            // No resources to dispose
        }
    }
}
