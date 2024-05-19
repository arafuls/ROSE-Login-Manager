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

            if (File.Exists(_configFile))
            {
                LoadConfig();
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
                XmlDocument doc = new();
                doc.Load(_configFile);

                // Get the RoseGameFolder element from the XML document
                XmlNode? roseGameFolderNode = doc.SelectSingleNode("//Configuration/RoseGameFolder");
                if (roseGameFolderNode != null)
                {
                    // Set the RoseGameFolderPath property from the XML node value
                    GlobalVariables.Instance.RoseGameFolder = roseGameFolderNode.InnerText;
                }
                else
                {
                    // If RoseGameFolder element does not exist, show a message or handle it accordingly
                    MessageBox.Show("RoseGameFolder element not found in the configuration file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading configuration file: " + ex.Message);
            }
        }



        /// <summary>
        ///     Creates a new configuration file if it does not exist.
        /// </summary>
        private void CreateConfig()
        {
            try
            {
                XmlDocument doc = new();
                XmlElement root = doc.CreateElement("Configuration");
                doc.AppendChild(root);
                doc.Save(_configFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating configuration file: " + ex.Message);
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
                XmlDocument doc = new();
                XmlElement root = doc.CreateElement("Configuration");
                doc.AppendChild(root);

                // Create the RoseGameFolder element and set its value
                XmlElement roseGameFolderElement = doc.CreateElement("RoseGameFolder");
                string roseGameFolderPath = GlobalVariables.Instance.RoseGameFolder ?? "";
                roseGameFolderElement.InnerText = roseGameFolderPath;
                root.AppendChild(roseGameFolderElement);

                // Save the XML document to file
                doc.Save(_configFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving configuration file: " + ex.Message);
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
