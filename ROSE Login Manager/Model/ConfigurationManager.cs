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
            _configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{GlobalVariables.APP_NAME}", "config.xml");
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

                XmlNode? generalSettingsNode = EnsureXmlNodeExists(_doc, "//Configuration/GeneralSettings");
                WeakReferenceMessenger.Default.Send(new SettingChangedMessage<string>("RoseGameFolder", GetConfigSetting("RoseGameFolder", generalSettingsNode, "")));
                WeakReferenceMessenger.Default.Send(new SettingChangedMessage<bool>("DisplayEmail", bool.Parse(GetConfigSetting("DisplayEmail", generalSettingsNode, true))));
                WeakReferenceMessenger.Default.Send(new SettingChangedMessage<bool>("MaskEmail", bool.Parse(GetConfigSetting("MaskEmail", generalSettingsNode, false))));
                HandleRoseInstallLocation(generalSettingsNode);

                XmlNode? gameSettingsNode = EnsureXmlNodeExists(_doc, "//Configuration/GameSettings");
                WeakReferenceMessenger.Default.Send(new SettingChangedMessage<bool>("SkipPlanetCutscene", bool.Parse(GetConfigSetting("SkipPlanetCutscene", gameSettingsNode, false))));
                WeakReferenceMessenger.Default.Send(new SettingChangedMessage<string>("LoginScreen", GetConfigSetting("LoginScreen", gameSettingsNode, "Random")));

                _doc.Save(_configFile);
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: $"{GlobalVariables.APP_NAME} - ConfigurationManager::LoadConfig",
                    message: "Error loading config: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
        }



        /// <summary>
        ///     Ensures the existence of an XML node specified by the XPath. If the node doesn't exist,
        ///     it creates the node under the parent node specified by the XPath.
        /// </summary>
        /// <param name="doc">The XML document.</param>
        /// <param name="xpath">The XPath of the node to ensure existence.</param>
        /// <returns>The existing or newly created XML node.</returns>
        private static XmlNode? EnsureXmlNodeExists(XmlDocument doc, string xpath)
        {
            XmlNode? node = doc.SelectSingleNode(xpath);
            if (node == null)
            {
                XmlNode? parentNode = doc.SelectSingleNode("//Configuration");
                if (parentNode != null)
                {
                    node = parentNode.AppendChild(doc.CreateElement(xpath[(xpath.LastIndexOf('/') + 1)..]));
                }
            }
            return node;
        }



        /// <summary>
        ///     Handles the ROSE install location by attempting to automatically find it.
        /// </summary>
        /// <param name="node">The XML node representing the settings.</param>
        private static void HandleRoseInstallLocation(XmlNode node)
        {
            // Attempt to automatically find ROSE install location - Thanks ZeroPoke :D
            if (string.IsNullOrEmpty(GlobalVariables.Instance.RoseGameFolder))
            {
                string folderLocation = GlobalVariables.InstallLocationFromRegistry;
                if (!string.IsNullOrEmpty(folderLocation))
                {
                    SaveSetting("RoseGameFolder", folderLocation, node);
                }
            }
        }



        /// <summary>
        ///     Creates a new configuration file with default settings.
        /// </summary>
        private void CreateConfig()
        {
            try
            {
                // Ensure the directory exists
                string? directory = Path.GetDirectoryName(_configFile);
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                XmlElement root = _doc.CreateElement("Configuration");
                _doc.AppendChild(root);

                XmlElement generalSettings = _doc.CreateElement("GeneralSettings");
                root.AppendChild(generalSettings);
                SaveConfigSetting("RoseGameFolder", "", generalSettings);
                SaveConfigSetting("DisplayEmail", "False", generalSettings);
                SaveConfigSetting("MaskEmail", "False", generalSettings);
                HandleRoseInstallLocation(generalSettings);

                XmlElement gameSettings = _doc.CreateElement("GameSettings");
                root.AppendChild(gameSettings);
                SaveConfigSetting("SkipPlanetCutscene", "False", gameSettings);
                SaveConfigSetting("LoginScreen", "Random", gameSettings);

                _doc.Save(_configFile);
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: $"{GlobalVariables.APP_NAME} - ConfigurationManager::CreateConfig",
                    message: "Error creating configuration file: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
        }



        /// <summary>
        ///     Saves a configuration setting with the specified key and value in the specified XML node.
        /// </summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        /// <param name="parentNode">The XML node where the setting should be saved.</param>
        private static void SaveSetting(string key, string value, XmlNode parentNode)
        {
            try
            {
                if (parentNode == null || parentNode.OwnerDocument == null)
                    return;

                XmlDocument ownerDocument = parentNode.OwnerDocument;
                XmlNode? existingSetting = parentNode.SelectSingleNode(key);

                if (existingSetting != null && existingSetting.InnerText == value)
                    return;

                if (existingSetting == null)
                {
                    // If the setting does not exist, create a new setting element.
                    XmlElement element = ownerDocument.CreateElement(key);
                    element.InnerText = value;
                    parentNode.AppendChild(element);
                }
                else
                {
                    // If the setting exists, update its value.
                    existingSetting.InnerText = value;
                }

                WeakReferenceMessenger.Default.Send(new SettingChangedMessage<string>(key, value));
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: $"{GlobalVariables.APP_NAME} - ConfigurationManager::SaveSetting",
                    message: "Error saving setting: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
        }



        /// <summary>
        ///     Saves a configuration setting with the specified key and value in the specified XML node.
        /// </summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        /// <param name="parentNode">The XML node where the setting should be saved.</param>
        public static void SaveConfigSetting(string key, object value, XmlNode parentNode)
        {
            SaveSetting(key, value.ToString(), parentNode);
        }




        /// <summary>
        ///     Saves a configuration setting with the specified key and value under the provided parent XML node.
        /// </summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        /// <param name="parentNodeName">The name of the parent XML node under which the setting will be saved.</param>
        public void SaveConfigSetting(string key, object value, string parentNodeName)
        {
            try
            {   // Construct the XPath based on the provided parent node name
                string parentNodeXPath = $"//Configuration/{parentNodeName}";
                XmlNode? parentNode = _doc.SelectSingleNode(parentNodeXPath);

                // Create the parent node if it doesn't exist
                if (parentNode == null)
                {
                    parentNode = _doc.CreateElement(parentNodeName);
                    _doc.SelectSingleNode("//Configuration")?.AppendChild(parentNode);
                }

                // Save the setting to the determined XML node if it's not null
                if (parentNode != null)
                {
                    SaveSetting(key, value.ToString(), parentNode);
                }
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: $"{GlobalVariables.APP_NAME} - ConfigurationManager::SaveConfigSetting",
                    message: "Error saving setting: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
            _doc.Save(_configFile);
        }




        /// <summary>
        ///     Retrieves the value of a configuration setting specified by the provided key from the given XML node. If the setting node does not exist, it creates a new one with the specified default value.
        /// </summary>
        /// <param name="key">The key of the setting to retrieve.</param>
        /// <param name="parentNode">The XML node where the setting is located or will be created.</param>
        /// <param name="defaultValue">The default value to assign to the setting if it doesn't exist.</param>
        /// <returns>The value of the configuration setting.</returns>
        public static string? GetConfigSetting(string key, XmlNode parentNode, object defaultValue)
        {
            XmlNode? settingNode = parentNode.SelectSingleNode(key);
            if (settingNode == null && parentNode.OwnerDocument != null)
            {
                settingNode = parentNode.OwnerDocument.CreateElement(key);
                settingNode.InnerText = defaultValue?.ToString() ?? string.Empty;
                parentNode.AppendChild(settingNode);
            }
            return settingNode?.InnerText;
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
