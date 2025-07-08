namespace ROSE_Login_Manager.Services.Interfaces;

/// <summary>
/// Service for detecting and managing ROSE Online client installation
/// </summary>
public interface IRoseClientService
{
    /// <summary>
    /// Gets the ROSE Online client installation path from registry
    /// </summary>
    /// <returns>The installation path if found, null otherwise</returns>
    string? GetClientInstallPath();

    /// <summary>
    /// Gets the path to the rose-updater.exe file
    /// </summary>
    /// <returns>The updater path if found, null otherwise</returns>
    string? GetUpdaterPath();

    /// <summary>
    /// Checks if the ROSE Online client is installed
    /// </summary>
    /// <returns>True if client is found, false otherwise</returns>
    bool IsClientInstalled();

    /// <summary>
    /// Checks if the updater is available
    /// </summary>
    /// <returns>True if updater is found, false otherwise</returns>
    bool IsUpdaterAvailable();

    /// <summary>
    /// Gets the main game executable path
    /// </summary>
    /// <returns>The rose.exe path if found, null otherwise</returns>
    string? GetGameExecutablePath();

    /// <summary>
    /// Debug method to list all ROSE Online related registry entries
    /// </summary>
    void DebugRegistryEntries();
} 