using ROSE_Login_Manager.Models;

namespace ROSE_Login_Manager.Services.Interfaces;

/// <summary>
/// Service for managing launcher settings stored in JSON configuration
/// </summary>
public interface ILauncherSettingsService
{
    /// <summary>
    /// Loads launcher settings from the configuration file
    /// </summary>
    /// <returns>The loaded settings or default settings if file doesn't exist</returns>
    Task<LauncherSettings> LoadSettingsAsync();

    /// <summary>
    /// Saves launcher settings to the configuration file
    /// </summary>
    /// <param name="settings">The settings to save</param>
    Task SaveSettingsAsync(LauncherSettings settings);

    /// <summary>
    /// Gets the path to the settings file
    /// </summary>
    /// <returns>The settings file path</returns>
    string GetSettingsFilePath();

    /// <summary>
    /// Resets settings to default values
    /// </summary>
    /// <returns>The default settings</returns>
    LauncherSettings GetDefaultSettings();

    /// <summary>
    /// Creates a backup of the settings file
    /// </summary>
    /// <param name="backupPath">Path where to save the backup</param>
    Task CreateBackupAsync(string backupPath);

    /// <summary>
    /// Restores settings from backup
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    Task RestoreFromBackupAsync(string backupPath);
} 