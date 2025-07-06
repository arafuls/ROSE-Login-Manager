namespace ROSE_Login_Manager.Models;

/// <summary>
/// Global launcher settings stored in JSON configuration
/// </summary>
public class LauncherSettings
{
    /// <summary>
    /// The current theme for the launcher
    /// </summary>
    public string Theme { get; set; } = "Default";

    /// <summary>
    /// Default path to the Rose Online game client
    /// </summary>
    public string DefaultGameClientPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether to start the launcher minimized
    /// </summary>
    public bool StartMinimized { get; set; } = false;

    /// <summary>
    /// Whether to check for updates on startup
    /// </summary>
    public bool CheckForUpdatesOnStartup { get; set; } = true;

    /// <summary>
    /// Whether to enable auto-login functionality
    /// </summary>
    public bool EnableAutoLogin { get; set; } = true;

    /// <summary>
    /// Delay in milliseconds before attempting auto-login
    /// </summary>
    public int AutoLoginDelay { get; set; } = 3000;

    /// <summary>
    /// Whether to close the launcher after launching the game
    /// </summary>
    public bool CloseAfterLaunch { get; set; } = false;

    /// <summary>
    /// Whether to minimize to system tray instead of closing
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Whether to show notifications
    /// </summary>
    public bool ShowNotifications { get; set; } = true;

    /// <summary>
    /// Language setting for the launcher
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// Date and time when settings were last modified
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
} 