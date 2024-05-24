using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using System.IO;
using System.Windows;
using System.Xml;



namespace ROSE_Login_Manager.Model
{
    /// <summary>
    ///     Represents a singleton class responsible for managing and storing configuration settings.
    /// </summary>
    class ConfigurationManager : IDisposable
    {
        private readonly string _configFile;
        private readonly XmlDocument _doc;



        /// <summary>
        ///     Gets the singleton instance of the ConfigurationManager class.
        /// </summary>
        private static readonly Lazy<ConfigurationManager> lazyInstance = new(() => new ConfigurationManager());
        public static ConfigurationManager Instance => lazyInstance.Value;



        /// <summary>
        ///     Private constructor to prevent external instantiation and initialize the ConfigurationManager.
        /// </summary>
        private ConfigurationManager()
        {
            _configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ROSE Online Login Manager", "config.xml");
            _doc = new XmlDocument();

            if (File.Exists(_configFile))
            {
                LoadConfig();
            }
            else
            {
                CreateConfig();
            }
        }



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
                    WeakReferenceMessenger.Default.Send(new SettingChangedMessage<string>("RoseGameFolder", GetConfigSetting("RoseGameFolder")));
                    WeakReferenceMessenger.Default.Send(new SettingChangedMessage<bool>("DisplayEmail", bool.Parse(GetConfigSetting("DisplayEmail"))));
                    WeakReferenceMessenger.Default.Send(new SettingChangedMessage<bool>("MaskEmail", bool.Parse(GetConfigSetting("MaskEmail"))));
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
                        if (existingSetting.InnerText == value) { return; }

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

                    WeakReferenceMessenger.Default.Send(new SettingChangedMessage<string>(key, value));
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
