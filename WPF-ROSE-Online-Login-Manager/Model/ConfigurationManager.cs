using ROSE_Online_Login_Manager.Resources.Util;
using System.IO;
using System.Windows;
using System.Xml;



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



        #region Singleton
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
                _doc.Load(_configFile);
            }
            else
            {
                CreateConfig();
            }

            _globalVariables.RoseGameFolderChanged += HandleRoseGameFolderChanged;
        }



        /// <summary>
        ///     Gets the singleton instance of ConfigurationManager.
        /// </summary>
        public static ConfigurationManager Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion



        /// <summary>
        ///     Loads configuration settings from an existing file.
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                // Get the RoseGameFolder element from the XML document
                XmlNode? roseGameFolderNode = _doc.SelectSingleNode("//Configuration/RoseGameFolder");
                if (roseGameFolderNode != null)
                {
                    // Set the RoseGameFolderPath property from the XML node value
                    _globalVariables.RoseGameFolder = roseGameFolderNode.InnerText;
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
        ///     Creates a new configuration file if it does not exist.
        /// </summary>
        private void CreateConfig()
        {
            try
            {
                XmlElement root = _doc.CreateElement("Configuration");
                _doc.AppendChild(root);

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
        ///     Handles the event raised when the RoseGameFolder path in GlobalVariables changes.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleRoseGameFolderChanged(object sender, EventArgs e)
        {
            SaveRoseGameFolderPath();
        }



        /// <summary>
        ///     Saves the RoseGameFolder string to the configuration file.
        /// </summary>
        public void SaveRoseGameFolderPath()
        {
            try
            {
                // Get or create the root element
                XmlElement? rootElement = _doc.DocumentElement;
                if (rootElement == null)
                {
                    rootElement = _doc.CreateElement("Configuration");
                    _doc.AppendChild(rootElement);
                }

                // Get or create the RoseGameFolder element
                if (rootElement.SelectSingleNode("RoseGameFolder") is not XmlElement roseGameFolderElement)
                {
                    roseGameFolderElement = _doc.CreateElement("RoseGameFolder");
                    rootElement.AppendChild(roseGameFolderElement);
                }

                // Set the RoseGameFolder element value
                string roseGameFolderPath = GlobalVariables.Instance.RoseGameFolder ?? "";
                roseGameFolderElement.InnerText = roseGameFolderPath;

                // Save the XML document to file
                _doc.Save(_configFile);
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - ConfigurationManager::SaveRoseGameFolderPath",
                    message: "Error saving configuration file: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
        }



        /// <summary>
        ///     Releases all resources used by the ConfigurationManager.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
