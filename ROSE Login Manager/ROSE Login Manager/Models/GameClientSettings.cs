using System.Collections.Generic;

namespace ROSE_Login_Manager.Models;

/// <summary>
/// Flexible model for ROSE Online game client settings stored in TOML files
/// </summary>
public class GameClientSettings
{
    /// <summary>
    /// Dictionary to store all TOML settings as key-value pairs
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();

    /// <summary>
    /// Path to the TOML file
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Last modified time of the file
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a setting value by key
    /// </summary>
    /// <typeparam name="T">The expected type of the value</typeparam>
    /// <param name="key">The setting key</param>
    /// <param name="defaultValue">Default value if key doesn't exist</param>
    /// <returns>The setting value or default</returns>
    public T GetSetting<T>(string key, T defaultValue = default!)
    {
        if (Settings.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// Sets a setting value
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="value">The setting value</param>
    public void SetSetting(string key, object value)
    {
        Settings[key] = value;
        LastModified = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a setting exists
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <returns>True if the setting exists</returns>
    public bool HasSetting(string key)
    {
        return Settings.ContainsKey(key);
    }

    /// <summary>
    /// Removes a setting
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <returns>True if the setting was removed</returns>
    public bool RemoveSetting(string key)
    {
        var removed = Settings.Remove(key);
        if (removed)
        {
            LastModified = DateTime.UtcNow;
        }
        return removed;
    }
} 