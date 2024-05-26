using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Data.Sqlite;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;



namespace ROSE_Login_Manager.Resources.Util
{
    /// <summary>
    ///     Message indicating that the database has changed.
    /// </summary>
    public class DatabaseChangedMessage { }



    /// <summary>
    ///     Manages interactions with the <see cref="SqliteConnection"/>.
    /// </summary>
    internal class DatabaseManager : IDisposable
    {
        private const string DB_FILENAME = "data.sqlite";
        private readonly string _dbFilePath;
        private readonly string _appFolderPath;
        private readonly SqliteConnection _db;



        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseManager"/> class.
        /// </summary>
        public DatabaseManager()
        {
            _appFolderPath = GlobalVariables.Instance.AppPath;
            _dbFilePath = Path.Combine(_appFolderPath, DB_FILENAME);
            _db = new SqliteConnection($"Data Source={_dbFilePath}");
            InitializeAppFolder();
            InitializeDatabase();
        }



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
            CreateSchema();
        }



        /// <summary>
        ///     Creates the database schema if it does not already exist.
        /// </summary>
        private void CreateSchema()
        {
            _db.Open();
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
            _db.Close();
        }



        /// <summary>
        ///     Executes a specified SQL command that does not return any data.
        /// </summary>
        /// <param name="command">The <see cref="SqliteCommand"/> to execute.</param>
        /// <returns>True if the command executed successfully; otherwise, false.</returns>
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
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - DatabaseManager::ExecuteNonQuery",
                    message: $"SQLite Error {ex.SqliteErrorCode}: '{ex.Message}'",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
                result = false;
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - DatabaseManager::ExecuteNonQuery",
                    message: $"An error occurred: {ex.Message}",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
                result = false;
            }
            finally
            {
                _db.Close();
            }

            return result;
        }



        /// <summary>
        ///     Retrieves all user profiles from the database.
        /// </summary>
        /// <returns>An <see cref="ObservableCollection{T}"/> of <see cref="UserProfileModel"/> representing all user profiles in the database.</returns>
        internal ObservableCollection<UserProfileModel> GetAllProfiles()
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
        ///     Adds a new user profile to the database.
        /// </summary>
        /// <param name="profile">The user profile to add.</param>
        /// <returns>
        ///     True if the profile was added successfully; otherwise, false.
        /// </returns>
        /// <remarks>
        ///     This method inserts a new profile into the Profiles table in the database.
        ///     It sends a <see cref="DatabaseChangedMessage"/> using the WeakReferenceMessenger
        ///     if the operation is successful to inform ViewModels that use profile data.
        /// </remarks>
        internal bool AddProfile(UserProfileModel profile)
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

            if (ExecuteNonQuery(command))
            {
                WeakReferenceMessenger.Default.Send(new DatabaseChangedMessage());
                return true;
            }

            return false;
        }



        /// <summary>
        ///     Updates an existing user profile in the database.
        /// </summary>
        /// <param name="profile">The user profile to update.</param>
        /// <returns>
        ///     True if the profile was updated successfully; otherwise, false.
        /// </returns>
        /// <remarks>
        ///     This method updates the profile details in the Profiles table in the database.
        ///     It sends a <see cref="DatabaseChangedMessage"/> using the WeakReferenceMessenger
        ///     if the operation is successful to inform ViewModels that use profile data.
        /// </remarks>
        internal bool UpdateProfile(UserProfileModel profile)
        {
            using SqliteCommand command = _db.CreateCommand();
            command.CommandText = @"
                UPDATE Profiles 
                SET ProfileStatus = @ProfileStatus, 
                    ProfileName = @ProfileName, 
                    ProfilePassword = @ProfilePassword, 
                    ProfileIV = @ProfileIV
                WHERE ProfileEmail = @ProfileEmail
            ";

            command.Parameters.AddWithValue("@ProfileStatus", profile.ProfileStatus);
            command.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);
            command.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
            command.Parameters.AddWithValue("@ProfilePassword", profile.ProfilePassword);
            command.Parameters.AddWithValue("@ProfileIV", profile.ProfileIV);

            if (ExecuteNonQuery(command))
            {
                WeakReferenceMessenger.Default.Send(new DatabaseChangedMessage());
                return true;
            }

            return false;
        }



        /// <summary>
        ///     Deletes a user profile from the database based on the provided email.
        /// </summary>
        /// <param name="profileEmail">The email of the profile to delete.</param>
        /// <returns>
        ///     True if the profile was deleted successfully; otherwise, false.
        /// </returns>
        /// <remarks>
        ///     This method deletes the profile with the specified email from the Profiles table in the database.
        ///     It sends a <see cref="DatabaseChangedMessage"/> using the WeakReferenceMessenger
        ///     if the operation is successful to inform ViewModels that use profile data.
        /// </remarks>

        internal bool DeleteProfile(string profileEmail)
        {
            using SqliteCommand command = _db.CreateCommand();
            command.CommandText = "DELETE FROM Profiles WHERE ProfileEmail = @ProfileEmail";
            command.Parameters.AddWithValue("@ProfileEmail", profileEmail);

            if (ExecuteNonQuery(command))
            {
                WeakReferenceMessenger.Default.Send(new DatabaseChangedMessage());
                return true;
            }

            return false;
        }



        /// <summary>
        ///     Checks if there is a potential record collision for the given email in the database.
        /// </summary>
        /// <param name="email">The email to check for potential collisions.</param>
        /// <returns>
        ///     True if a record with the specified email already exists in the database; otherwise, false.
        /// </returns>
        /// <remarks>
        ///     This method executes a SQL query to count the number of records with the specified email
        ///     in the Profiles table. If the count is greater than zero, it indicates a potential record collision.
        /// </remarks>
        internal bool PotentialRecordCollision(string email)
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
        ///     Retrieves the ProfileIV from the database based on the provided email.
        /// </summary>
        /// <param name="email">The email associated with the profile.</param>
        /// <returns>
        ///     The ProfileIV corresponding to the provided email. Returns an empty string if the email is not found.
        /// </returns>
        /// <remarks>
        ///     This method executes a SQL query to fetch the ProfileIV for the specified email from the Profiles table.
        ///     If the email does not exist in the database, the method returns an empty string.
        /// </remarks>
        internal string GetProfileIVByEmail(string email)
        {
            string query = "SELECT ProfileIV FROM Profiles WHERE ProfileEmail = @Email";
            string profileIV = string.Empty;

            _db.Open();
            using (SqliteCommand command = _db.CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.AddWithValue("@Email", email);

                // Execute the query to fetch the ProfileIV
                object? result = command.ExecuteScalar();
                profileIV = result?.ToString() ?? string.Empty;
            }
            _db.Close();

            return profileIV;
        }
        #endregion



        #region IDisposable Implementation
        /// <summary>
        ///     Releases all resources used by the DatabaseManager.
        /// </summary>
        /// <remarks>
        ///     This method calls the protected <see cref="Dispose(bool)"/> method with the disposing parameter set to true
        ///     and then suppresses finalization for the object. This is part of the implementation of the IDisposable interface.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }



        /// <summary>
        ///     Releases the unmanaged resources used by the DatabaseManager and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     If set to <c>true</c>, the method releases both managed and unmanaged resources. 
        ///     If set to <c>false</c>, the method releases only unmanaged resources.
        /// </param>
        /// <remarks>
        ///     This method is called by the public <see cref="Dispose()"/> method and the finalizer.
        ///     When disposing is true, this method disposes of the managed resources (such as the database connection).
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
        }



        /// <summary>
        ///     Finalizer for the DatabaseManager class.
        /// </summary>
        /// <remarks>
        ///     This destructor is called by the garbage collector when the object is being finalized. 
        ///     It calls the <see cref="Dispose(bool)"/> method with the disposing parameter set to false.
        /// </remarks>
        ~DatabaseManager()
        {
            Dispose(false);
        }
        #endregion
    }
}
