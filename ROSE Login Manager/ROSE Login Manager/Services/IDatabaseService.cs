using ROSE_Login_Manager.Models;

namespace ROSE_Login_Manager.Services;

/// <summary>
/// Service for managing SQLite database operations
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Initializes the database and creates tables if they don't exist
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Gets the database connection string
    /// </summary>
    string GetConnectionString();

    /// <summary>
    /// Creates a new database connection
    /// </summary>
    /// <returns>Database connection</returns>
    Microsoft.Data.Sqlite.SqliteConnection CreateConnection();

    /// <summary>
    /// Checks if the database exists
    /// </summary>
    /// <returns>True if database exists</returns>
    bool DatabaseExists();

    /// <summary>
    /// Creates a backup of the database
    /// </summary>
    /// <param name="backupPath">Path where to save the backup</param>
    Task CreateBackupAsync(string backupPath);

    /// <summary>
    /// Restores database from backup
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    Task RestoreFromBackupAsync(string backupPath);
} 