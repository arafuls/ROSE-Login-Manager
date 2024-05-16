using Microsoft.Data.Sqlite;
using ROSE_Online_Login_Manager.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ROSE_Online_Login_Manager.Resources.Util
{
    class DatabaseManager : IDisposable
    {
        private const string DB_FILENAME = "data.sqlite";

        private string _dbFilePath    = "";
        private string _appFolderPath = "";
        private SqliteConnection _db = new();



        #region Event Handlers
        public event EventHandler? ProfileAdded;
        protected virtual void OnProfileAdded()
        {
            ProfileAdded?.Invoke(this, EventArgs.Empty);
        }
        #endregion



        /// <summary>
        ///     Default Constructor
        /// </summary>
        public DatabaseManager()
        {
            InitializeAppFolder();
            InitializeDatabase();
        }



        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }



        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                _db?.Dispose();
            }

            // Dispose unmanaged resources
        }



        ~DatabaseManager()
        {
            Dispose(false);
        }
        #endregion



        #region Methods
        /// <summary>
        ///     Sets the database filepath, connection, and schema.
        /// </summary>
        private void InitializeDatabase()
        {
            _dbFilePath = Path.Combine(_appFolderPath, DB_FILENAME);    // Get database file path
            _db = new SqliteConnection($"Data Source={_dbFilePath}");   // Establish database connection

            CreateSchema();
        }



        /// <summary>
        ///     Creates the database schema if it does not already exist.
        /// </summary>
        private void CreateSchema()
        {
            using SqliteCommand command = _db.CreateCommand();
            command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Profiles (
                        ProfileStatus       INTEGER     NOT NULL,
                        ProfileEmail        TEXT        PRIMARY KEY,
                        ProfileName         TEXT        NOT NULL,
                        ProfilePassword     TEXT        NOT NULL,
                        ProfileIV           TEXT        NOT NULL
                    );
                ";
            ExecuteNonQuery(command);
        }



        /// <summary>
        ///     Creates the App Folder if it does not exists and sets the
        ///     _appFolderPath.
        /// </summary>
        private void InitializeAppFolder()
        {
            // Get the app data folder within %appdata%
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ROSE Online Login Manager");

            // Create the app data folder
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Set the app folder path
            _appFolderPath = folderPath;
        }



        /// <summary>
        ///     Retrieves all profiles from the database.
        /// </summary>
        /// <returns>A list of UserProfileModel objects representing the profiles.</returns>
        public ObservableCollection<UserProfileModel> GetAllProfiles()
        {
            ObservableCollection<UserProfileModel> profiles = [];

            _db.Open();
            using (SqliteCommand command = _db.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Profiles";
                using SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    UserProfileModel profile = new()
                    {
                        ProfileStatus       = reader.GetBoolean(0),
                        ProfileEmail        = reader.GetString(1),
                        ProfileName         = reader.GetString(2),
                        ProfilePassword     = reader.GetString(3),
                        ProfileIV           = reader.GetString(4)
                    };
                    profiles.Add(profile);
                }
            }
            _db.Close();
            return profiles;
        }



        /// <summary>
        ///     Attempts to insert a UserProfileModel entry into the database.
        /// </summary>
        /// <returns>A boolean representing success.</returns>
        public bool InsertProfileIntoDatabase(UserProfileModel profile)
        {
            using SqliteCommand command = _db.CreateCommand();

            // Create a SQL command to insert the profile data into the Users table
            command.CommandText = @"
                        INSERT INTO Profiles (ProfileStatus, ProfileEmail, ProfileName, ProfilePassword, ProfileIV)
                        VALUES (@ProfileStatus, @ProfileEmail, @ProfileName, @ProfilePassword, @ProfileIV)
                    ";

            // Add parameters to the command to prevent SQL injection and handle special characters
            command.Parameters.AddWithValue("@ProfileStatus", profile.ProfileStatus);
            command.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);
            command.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
            command.Parameters.AddWithValue("@ProfilePassword", profile.ProfilePassword);
            command.Parameters.AddWithValue("@ProfileIV", profile.ProfileIV);

            return ExecuteNonQuery(command);
        }



        public bool PotentialRecordCollision(string email)
        {
            bool collisionDetected = false;

            // SQL query to check for existing entry with the same email
            string query = "SELECT COUNT(*) FROM Profiles WHERE ProfileEmail = @Email";

            _db.Open();
            using (SqliteCommand command = _db.CreateCommand())
            {
                // Create a SQL command to execute the query
                command.CommandText = query;
                command.Parameters.AddWithValue("@Email", email);

                // Execute the query and check the result
                int count = Convert.ToInt32(value: command.ExecuteScalar());
                collisionDetected = count > 0;
            }
            _db.Close();

            return collisionDetected;
        }



        internal void DeleteProfile(UserProfileModel profile)
        {
            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            using SqliteCommand command = _db.CreateCommand();
            command.CommandText = "DELETE FROM Profiles WHERE ProfileEmail = @ProfileEmail";
            command.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);
            ExecuteNonQuery(command);
        }


        private bool ExecuteNonQuery(SqliteCommand command)
        {
            _db.Open();

            bool queryResult = true;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) // Handle the exception
            {
                MessageBox.Show($"SQLite Error {ex.SqliteErrorCode}: '{ex.Message}'", "SQLite Error", MessageBoxButton.OK, MessageBoxImage.Error);
                queryResult = false;
            }
            catch (Exception ex) // Handle other exceptions
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                queryResult = false;
            }

            _db.Close();
            return queryResult;
        }
        #endregion
    }
}
