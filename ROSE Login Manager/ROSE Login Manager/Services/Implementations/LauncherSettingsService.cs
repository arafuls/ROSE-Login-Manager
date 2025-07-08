using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ROSE_Login_Manager.Models;
using ROSE_Login_Manager.Services.Interfaces;
using System.IO;

namespace ROSE_Login_Manager.Services.Implementations;

/// <summary>
/// Implementation of launcher settings service for JSON configuration management
/// </summary>
public class LauncherSettingsService : ILauncherSettingsService
{
    private readonly ILogger<LauncherSettingsService> _logger;
    private readonly string _settingsFilePath;

    public LauncherSettingsService(ILogger<LauncherSettingsService> logger)
    {
        _logger = logger;
        
        // Create settings directory if it doesn't exist
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RoseOnlineLoginManager"
        );
        
        if (!Directory.Exists(settingsDirectory))
        {
            Directory.CreateDirectory(settingsDirectory);
        }

        _settingsFilePath = Path.Combine(settingsDirectory, "LauncherSettings.json");
    }

    /// <summary>
    /// Gets the path to the settings file
    /// </summary>
    public string GetSettingsFilePath()
    {
        return _settingsFilePath;
    }

    /// <summary>
    /// Loads launcher settings from the configuration file
    /// </summary>
    public async Task<LauncherSettings> LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                _logger.LogInformation("Settings file not found, creating default settings");
                var defaultSettings = GetDefaultSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            var settings = JsonConvert.DeserializeObject<LauncherSettings>(json);

            if (settings == null)
            {
                _logger.LogWarning("Failed to deserialize settings, using defaults");
                return GetDefaultSettings();
            }

            _logger.LogInformation("Settings loaded successfully from {SettingsPath}", _settingsFilePath);
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings from {SettingsPath}", _settingsFilePath);
            return GetDefaultSettings();
        }
    }

    /// <summary>
    /// Saves launcher settings to the configuration file
    /// </summary>
    public async Task SaveSettingsAsync(LauncherSettings settings)
    {
        try
        {
            settings.LastModified = DateTime.UtcNow;
            
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            await File.WriteAllTextAsync(_settingsFilePath, json);

            _logger.LogInformation("Settings saved successfully to {SettingsPath}", _settingsFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings to {SettingsPath}", _settingsFilePath);
            throw;
        }
    }

    /// <summary>
    /// Resets settings to default values
    /// </summary>
    public LauncherSettings GetDefaultSettings()
    {
        return new LauncherSettings
        {
            Theme = "Default",
            DefaultGameClientPath = string.Empty,
            StartMinimized = false,
            CheckForUpdatesOnStartup = true,
            EnableAutoLogin = true,
            AutoLoginDelay = 3000,
            CloseAfterLaunch = false,
            MinimizeToTray = true,
            ShowNotifications = true,
            Language = "en-US",
            LastModified = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a backup of the settings file
    /// </summary>
    public async Task CreateBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                throw new FileNotFoundException("Settings file not found", _settingsFilePath);
            }

            var backupDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDirectory) && !Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            File.Copy(_settingsFilePath, backupPath, true);
            
            _logger.LogInformation("Settings backup created at {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create settings backup");
            throw;
        }
    }

    /// <summary>
    /// Restores settings from backup
    /// </summary>
    public async Task RestoreFromBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found", backupPath);
            }

            // Validate that the backup file contains valid JSON
            var json = await File.ReadAllTextAsync(backupPath);
            var settings = JsonConvert.DeserializeObject<LauncherSettings>(json);

            if (settings == null)
            {
                throw new InvalidOperationException("Backup file contains invalid JSON");
            }

            // Copy backup to settings location
            File.Copy(backupPath, _settingsFilePath, true);
            
            _logger.LogInformation("Settings restored from backup {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore settings from backup");
            throw;
        }
    }
} 