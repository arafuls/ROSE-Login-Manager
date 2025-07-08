using Microsoft.Data.Sqlite;
using ROSE_Login_Manager.Services.Interfaces;
using System.IO;

namespace ROSE_Login_Manager.Services.Implementations;

/// <summary>
/// Implementation of database service for SQLite operations
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "ROSE Login Manager");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _databasePath = Path.Combine(appFolder, "profiles.db");
        _connectionString = $"Data Source={_databasePath};Cache=Shared;";
    }

    public async Task InitializeAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS UserProfiles (
                Id TEXT PRIMARY KEY,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL,
                Server TEXT NOT NULL,
                IsDefault INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            )";

        await createTableCommand.ExecuteNonQueryAsync();
    }

    public string GetConnectionString()
    {
        return _connectionString;
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public bool DatabaseExists()
    {
        return File.Exists(_databasePath);
    }

    public async Task CreateBackupAsync(string backupPath)
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
    }

    public async Task RestoreFromBackupAsync(string backupPath)
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
    }
} 