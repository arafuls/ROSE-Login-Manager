using Microsoft.Data.Sqlite;
using ROSE_Online_Login_Manager.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;



namespace ROSE_Online_Login_Manager.Resources.Util
{
    /// <summary>
    ///     Manages interactions with the SQLite database.
    /// </summary>
    internal class DatabaseManager : IDisposable
    {
        private const string DB_FILENAME = "data.sqlite";
        private readonly string _dbFilePath;
        private readonly string _appFolderPath;
        private readonly SqliteConnection _db;



        #region Events
        public event EventHandler? ProfileAdded;
        protected virtual void OnProfileAdded() => ProfileAdded?.Invoke(this, EventArgs.Empty);
        #endregion



        /// <summary>
        ///     Initializes a new instance of the DatabaseManager class.
        /// </summary>
        public DatabaseManager()
        {
            _appFolderPath = GlobalVariables.Instance.AppPath;
            _dbFilePath = Path.Combine(_appFolderPath, DB_FILENAME);
            _db = new SqliteConnection($"Data Source={_dbFilePath}");
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
                _db?.Dispose();
            }
        }



        ~DatabaseManager()
        {
            Dispose(false);
        }
        #endregion



        #region Methods
        /// <summary>
        ///     Initializes the app folder.
        /// </summary>
        private void InitializeAppFolder()
        {
            if (!Directory.Exists(_appFolderPath))
            {
                Directory.CreateDirectory(_appFolderPath);
            }
        }



        /// <summary>
        ///     Initializes the database and creates the schema if necessary.
        /// </summary>
        private void InitializeDatabase()
        {
            _db.Open();
            CreateSchema();
            _db.Close();
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
        ///     Executes the specified SQL command that does not return any data.
        /// </summary>
        private bool ExecuteNonQuery(SqliteCommand command)
        {
            bool result = true;

            try
            {
                _db.Open();
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                result = false;
                ShowErrorMessage($"SQLite Error {ex.SqliteErrorCode}: '{ex.Message}'", "SQLite Error");
            }
            catch (Exception ex)
            {
                result = false;
                ShowErrorMessage($"An error occurred: {ex.Message}", "Error");
            }
            finally
            {
                _db.Close();
            }

            return result;
        }



        /// <summary>
        ///     Retrieves all profiles from the database.
        /// </summary>
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
                    UserProfileModel userProfileModel = new()
                    {
                        ProfileStatus = reader.GetBoolean(0),
                        ProfileEmail = reader.GetString(1),
                        ProfileName = reader.GetString(2),
                        ProfilePassword = reader.GetString(3),
                        ProfileIV = reader.GetString(4)
                    };
                    profiles.Add(userProfileModel);
                }
            }
            _db.Close();

            return profiles;
        }



        /// <summary>
        ///     Inserts a user profile into the database.
        /// </summary>
        /// <param name="profile">The user profile to insert.</param>
        /// <returns>True if the profile was inserted successfully; otherwise, false.</returns>
        public bool InsertProfileIntoDatabase(UserProfileModel profile)
        {
            using SqliteCommand command = _db.CreateCommand();
            command.CommandText = @"
                INSERT INTO Profiles (ProfileStatus, ProfileEmail, ProfileName, ProfilePassword, ProfileIV)
                VALUES (@ProfileStatus, @ProfileEmail, @ProfileName, @ProfilePassword, @ProfileIV)
            ";

            command.Parameters.AddWithValue("@ProfileStatus", profile.ProfileStatus);
            command.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);
            command.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
            command.Parameters.AddWithValue("@ProfilePassword", profile.ProfilePassword);
            command.Parameters.AddWithValue("@ProfileIV", profile.ProfileIV);

            return ExecuteNonQuery(command);
        }



        /// <summary>
        ///     Checks if there is a potential record collision for the given email.
        /// </summary>
        public bool PotentialRecordCollision(string email)
        {
            string query = "SELECT COUNT(*) FROM Profiles WHERE ProfileEmail = @Email";
            bool collisionDetected = false;

            _db.Open();
            using (SqliteCommand command = _db.CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.AddWithValue("@Email", email);

                int count = Convert.ToInt32(value: command.ExecuteScalar());
                collisionDetected = count > 0;
            }
            _db.Close();

            return collisionDetected;
        }



        /// <summary>
        ///     Deletes the specified profile from the database.
        /// </summary>
        internal void DeleteProfile(UserProfileModel profile)
        {
            using SqliteCommand command = _db.CreateCommand();
            command.CommandText = "DELETE FROM Profiles WHERE ProfileEmail = @ProfileEmail";
            command.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);
            ExecuteNonQuery(command);
        }



        /// <summary>
        ///     Displays an error message box with the specified message and title.
        /// </summary>
        private static void ShowErrorMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
    }
}
