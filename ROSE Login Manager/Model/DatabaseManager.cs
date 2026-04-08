using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Data.Sqlite;
using NLog;
using ROSE_Login_Manager.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string DB_FILENAME = "data.sqlite";
        private readonly string _dbFilePath;
        private readonly string _appFolderPath;



        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseManager"/> class.
        /// </summary>
        public DatabaseManager()
        {
            _appFolderPath = GlobalVariables.Instance.AppPath;
            _dbFilePath = Path.Combine(_appFolderPath, DB_FILENAME);
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
            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();

            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Profiles'";
            bool tableExists = Convert.ToInt32(command.ExecuteScalar()) > 0;

            if (tableExists)
            {
                VerifySchemaHasOrder(connection);
            }
            else
            {
                CreateSchema(connection);
            }
        }



        /// <summary>
        ///     Checks if the 'Profiles' table contains the 'ProfileOrder' column. If not, adds the column to the table.
        /// </summary>
        private void VerifySchemaHasOrder(SqliteConnection connection)
        {
            using SqliteCommand command = connection.CreateCommand();
            bool profileOrderExists = false;

            command.CommandText = "PRAGMA table_info('Profiles')";
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string columnName = reader.GetString(1);
                    if (columnName.Equals("ProfileOrder", StringComparison.OrdinalIgnoreCase))
                    {
                        profileOrderExists = true;
                        break;
                    }
                }
                reader.Close();
            }

            if (!profileOrderExists)
            {
                command.CommandText = "ALTER TABLE Profiles ADD COLUMN ProfileOrder INTEGER NOT NULL DEFAULT 0";
                command.ExecuteNonQuery();
            }
        }



        /// <summary>
        ///     Creates the database schema if it does not already exist.
        /// </summary>
        private void CreateSchema(SqliteConnection connection)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Profiles (
                    ProfileStatus       INTEGER     NOT NULL,
                    ProfileEmail        TEXT        PRIMARY KEY,
                    ProfileName         TEXT        NOT NULL,
                    ProfilePassword     TEXT        NOT NULL,
                    ProfileIV           TEXT        NOT NULL,
                    ProfileOrder        INTEGER     NOT NULL DEFAULT 0
                );
            ";
            ExecuteNonQuery(command, connection);
        }



        /// <summary>
        ///     Executes a specified SQL command that does not return any data.
        /// </summary>
        /// <param name="command">The <see cref="SqliteCommand"/> to execute.</param>
        /// <param name="connection">The <see cref="SqliteConnection"/> to use.</param>
        /// <returns>True if the command executed successfully; otherwise, false.</returns>
        private bool ExecuteNonQuery(SqliteCommand command, SqliteConnection connection)
        {
            bool result = true;

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                Logger.Error(ex, $"An SQL error while executing command: {command.CommandText}");
                result = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"An unexpected error occured while executing command: {command.CommandText}");
                result = false;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
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

            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Profiles";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                profiles.Add(new UserProfileModel
                {
                    ProfileStatus = reader.GetBoolean(0),
                    ProfileEmail = reader.GetString(1),
                    ProfileName = reader.GetString(2),
                    ProfilePassword = reader.GetString(3),
                    ProfileIV = reader.GetString(4),
                    ProfileOrder = reader.GetInt32(5)
                });
            }
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
            bool success = false;

            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            
            command.CommandText = "SELECT COALESCE(MAX(ProfileOrder), 0) + 1 FROM Profiles";
            int profileOrder = Convert.ToInt32(command.ExecuteScalar());

            command.CommandText = @"
                INSERT INTO Profiles (ProfileStatus, ProfileEmail, ProfileName, ProfilePassword, ProfileIV, ProfileOrder)
                VALUES (@ProfileStatus, @ProfileEmail, @ProfileName, @ProfilePassword, @ProfileIV, @ProfileOrder)
            ";

            command.Parameters.AddWithValue("@ProfileStatus", profile.ProfileStatus);
            command.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);
            command.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
            command.Parameters.AddWithValue("@ProfilePassword", profile.ProfilePassword);
            command.Parameters.AddWithValue("@ProfileIV", profile.ProfileIV);
            command.Parameters.AddWithValue("@ProfileOrder", profileOrder);

            if (ExecuteNonQuery(command, connection))
            {
                Logger.Info($"Profile {profile.ProfileName} was added successfully.");
                WeakReferenceMessenger.Default.Send(new DatabaseChangedMessage());
                success = true;
            }

            return success;
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
            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            using SqliteCommand command = connection.CreateCommand();
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

            if (ExecuteNonQuery(command, connection))
            {
                WeakReferenceMessenger.Default.Send(new DatabaseChangedMessage());
                return true;
            }

            return false;
        }



        /// <summary>
        ///     Updates the order of user profiles in the database based on the provided collection of profiles.
        /// </summary>
        /// <param name="profiles">The collection of user profiles containing the updated order.</param>
        /// <remarks>
        ///     This method updates the ProfileOrder property for each user profile in the database according to the order
        ///     specified in the provided collection. It opens a transaction to ensure atomicity and consistency when
        ///     updating profile orders. After updating the orders, it commits the transaction to save the changes permanently.
        ///     If any error occurs during the update process, it rolls back the transaction to maintain data integrity.
        ///     Finally, it sends a <see cref="DatabaseChangedMessage"/> using the WeakReferenceMessenger to notify
        ///     ViewModels that the database has changed.
        /// </remarks>
        /// <param name="profiles">The collection of user profiles containing the updated order.</param>
        internal void UpdateProfileOrder(ObservableCollection<UserProfileModel> profiles)
        {
            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            connection.Open();
            using SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                foreach (UserProfileModel profile in profiles)
                {
                    using SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "UPDATE Profiles SET ProfileOrder = @ProfileOrder WHERE ProfileEmail = @ProfileEmail";
                    command.Parameters.AddWithValue("@ProfileOrder", profile.ProfileOrder);
                    command.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);
                    command.ExecuteNonQuery();
                }
                transaction.Commit();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    WeakReferenceMessenger.Default.Send(new DatabaseChangedMessage());
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while updating profile order.");
                transaction.Rollback();
            }
        }



        /// <summary>
        ///     Updates the ProfileStatus of a user profile based on the provided email.
        /// </summary>
        /// <param name="email">The email of the profile to update.</param>
        /// <param name="status">The new status of the profile.</param>
        /// <returns>
        ///     True if the profile status was updated successfully; otherwise, false.
        /// </returns>
        internal bool UpdateProfileStatus(string email, bool status)
        {
            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Profiles 
                SET ProfileStatus = @ProfileStatus
                WHERE ProfileEmail = @ProfileEmail
            ";

            command.Parameters.AddWithValue("@ProfileStatus", status);
            command.Parameters.AddWithValue("@ProfileEmail", email);

            if (ExecuteNonQuery(command, connection))
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
            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Profiles WHERE ProfileEmail = @ProfileEmail";
            command.Parameters.AddWithValue("@ProfileEmail", profileEmail);

            if (ExecuteNonQuery(command, connection))
            {
                Logger.Info("Profile was deleted successfully.");
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

            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.AddWithValue("@Email", email);

            bool collisionDetected = Convert.ToInt32(command.ExecuteScalar()) > 0;

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

            using SqliteConnection connection = new($"Data Source={_dbFilePath}");
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.AddWithValue("@Email", email);

            string? iv = command.ExecuteScalar()?.ToString();

            return iv ?? string.Empty;
        }



        /// <summary>
        ///     Checks if an email exists in the database.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <returns>True if the email exists; otherwise, false.</returns>
        internal bool EmailExists(string email)
        {
            const string query = "SELECT COUNT(*) FROM Profiles WHERE ProfileEmail = @Email";

            try
            {
                using SqliteConnection connection = new($"Data Source={_dbFilePath}");
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = query;
                command.Parameters.AddWithValue("@Email", email);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while checking if email exists.");
                return false; 
            }
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
            GC.SuppressFinalize(this);
        }



        /// <summary>
        ///     Clears the status of all user profiles in the database, setting their ProfileStatus to false.
        /// </summary>
        /// <remarks>
        ///     This method opens a transaction to ensure atomicity and consistency when updating profile statuses.
        ///     It then executes an SQL command to update all profiles' statuses to false.
        ///     Finally, it commits the transaction to save the changes permanently.
        ///     If any error occurs during the process, it rolls back the transaction to maintain data integrity.
        /// </remarks>
        internal void ClearAllProfileStatus()
        {
            using SqliteConnection connection = new($"Data Source={_dbFilePath}");

            connection.Open();
            using SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Profiles SET ProfileStatus = 0";
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Error occurred while clearing profile status.");
                transaction.Rollback();
            }
        }



        /// <summary>
        ///     Retrieves the ProfileStatus from the database based on the provided email.
        /// </summary>
        /// <param name="email">The email associated with the profile.</param>
        /// <returns>The ProfileStatus corresponding to the provided email. Returns false if the email is not found.</returns>
        internal bool GetProfileStatusByEmail(string email)
        {
            const string query = "SELECT ProfileStatus FROM Profiles WHERE ProfileEmail = @Email";

            try
            {
                using SqliteConnection connection = new($"Data Source={_dbFilePath}");
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = query;
                command.Parameters.AddWithValue("@Email", email);

                object? result = command.ExecuteScalar();
                return result != null && result != DBNull.Value && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while getting profile status for email.");
                return false;
            }
        }



        /// <summary>
        ///     Checks if a user profile with the specified email exists in the database.
        /// </summary>
        /// <param name="email">The email address to check for in the database.</param>
        /// <returns>
        ///     <c>true</c> if a profile with the specified email exists; otherwise, <c>false</c>.
        /// </returns>
        internal bool ProfileExists(string email)
        {
            const string query = "SELECT COUNT(*) FROM Profiles WHERE ProfileEmail = @Email";

            try
            {
                using SqliteConnection connection = new($"Data Source={_dbFilePath}");
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = query;
                command.Parameters.AddWithValue("@Email", email);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while checking if profile exists.");
                return false;
            }
        }



        #endregion
    }
}
