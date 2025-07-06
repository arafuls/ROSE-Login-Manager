using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ROSE_Login_Manager.Services;

/// <summary>
/// Implementation of database service for SQLite operations
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly string _databasePath;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(ILogger<DatabaseService> logger)
    {
        _logger = logger;
        
        // Create App_Data directory if it doesn't exist
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RoseOnlineLoginManager"
        );
        
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _databasePath = Path.Combine(appDataPath, "LoginManager.db");
    }

    /// <summary>
    /// Gets the database connection string
    /// </summary>
    public string GetConnectionString()
    {
        return $"Data Source={_databasePath};Mode=ReadWriteCreate;";
    }

    /// <summary>
    /// Creates a new database connection
    /// </summary>
    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(GetConnectionString());
    }

    /// <summary>
    /// Checks if the database exists
    /// </summary>
    public bool DatabaseExists()
    {
        return File.Exists(_databasePath);
    }

    /// <summary>
    /// Initializes the database and creates tables if they don't exist
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Create UserProfiles table
            var createUserProfilesTable = @"
                CREATE TABLE IF NOT EXISTS UserProfiles (
                    Id TEXT PRIMARY KEY,
                    ProfileName TEXT NOT NULL,
                    GameAccountUsername TEXT NOT NULL,
                    EncryptedGameAccountPassword BLOB NOT NULL,
                    GameClientPath TEXT NOT NULL,
                    LauncherTheme TEXT,
                    CreatedAt TEXT NOT NULL,
                    ModifiedAt TEXT NOT NULL,
                    IsDefault INTEGER NOT NULL DEFAULT 0,
                    Notes TEXT
                )";

            using (var command = new SqliteCommand(createUserProfilesTable, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Create indexes for better performance
            var createIndexes = @"
                CREATE INDEX IF NOT EXISTS IX_UserProfiles_ProfileName ON UserProfiles(ProfileName);
                CREATE INDEX IF NOT EXISTS IX_UserProfiles_GameAccountUsername ON UserProfiles(GameAccountUsername);
                CREATE INDEX IF NOT EXISTS IX_UserProfiles_IsDefault ON UserProfiles(IsDefault);";

            using (var command = new SqliteCommand(createIndexes, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            _logger.LogInformation("Database initialized successfully at {DatabasePath}", _databasePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database");
            throw;
        }
    }

    /// <summary>
    /// Creates a backup of the database
    /// </summary>
    public async Task CreateBackupAsync(string backupPath)
    {
        try
        {
            if (!DatabaseExists())
            {
                throw new InvalidOperationException("Database does not exist");
            }

            var backupDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDirectory) && !Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            // Simple file copy for backup
            File.Copy(_databasePath, backupPath, true);
            
            _logger.LogInformation("Database backup created at {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database backup");
            throw;
        }
    }

    /// <summary>
    /// Restores database from backup
    /// </summary>
    public async Task RestoreFromBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found", backupPath);
            }

            // Close any existing connections
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Copy backup file to database location
            File.Copy(backupPath, _databasePath, true);
            
            _logger.LogInformation("Database restored from backup {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore database from backup");
            throw;
        }
    }
} 