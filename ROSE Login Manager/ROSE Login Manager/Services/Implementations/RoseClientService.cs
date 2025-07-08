using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using ROSE_Login_Manager.Services.Interfaces;
using System.IO;

namespace ROSE_Login_Manager.Services.Implementations;

/// <summary>
/// Service for detecting and managing Rose Online client installation
/// </summary>
public class RoseClientService : IRoseClientService
{
    private readonly ILogger<RoseClientService> _logger;
    private string? _cachedInstallPath;

    public RoseClientService(ILogger<RoseClientService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the Rose Online client installation path from registry
    /// </summary>
    /// <returns>The installation path if found, null otherwise</returns>
    public string? GetClientInstallPath()
    {
        const string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{975CAD98-4A32-4E44-8681-29A2C4BE0B93}_is1";
        const string valueName = "InstallLocation";
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(registryPath);
            if (key != null)
            {
                var value = key.GetValue(valueName) as string;
                if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
                {
                    _logger.LogInformation("Found Rose Online installation at: {Path}", value);
                    return value;
                }
            }
            _logger.LogWarning("Rose Online installation not found at registry key: {Key}", registryPath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading registry for Rose Online installation");
            return null;
        }
    }

    /// <summary>
    /// Gets the path to the rose-updater.exe file
    /// </summary>
    /// <returns>The updater path if found, null otherwise</returns>
    public string? GetUpdaterPath()
    {
        var installPath = GetClientInstallPath();
        if (string.IsNullOrEmpty(installPath))
            return null;

        var updaterPath = Path.Combine(installPath, "rose-updater.exe");
        if (File.Exists(updaterPath))
        {
            _logger.LogInformation("Found rose-updater.exe at: {Path}", updaterPath);
            return updaterPath;
        }

        _logger.LogWarning("rose-updater.exe not found at: {Path}", updaterPath);
        return null;
    }

    /// <summary>
    /// Checks if the Rose Online client is installed
    /// </summary>
    /// <returns>True if client is found, false otherwise</returns>
    public bool IsClientInstalled()
    {
        return !string.IsNullOrEmpty(GetClientInstallPath());
    }

    /// <summary>
    /// Checks if the updater is available
    /// </summary>
    /// <returns>True if updater is found, false otherwise</returns>
    public bool IsUpdaterAvailable()
    {
        return !string.IsNullOrEmpty(GetUpdaterPath());
    }

    /// <summary>
    /// Gets the main game executable path
    /// </summary>
    /// <returns>The rose.exe path if found, null otherwise</returns>
    public string? GetGameExecutablePath()
    {
        var installPath = GetClientInstallPath();
        if (string.IsNullOrEmpty(installPath))
            return null;

        var gameExePath = Path.Combine(installPath, "rose.exe");
        if (File.Exists(gameExePath))
        {
            _logger.LogInformation("Found rose.exe at: {Path}", gameExePath);
            return gameExePath;
        }

        _logger.LogWarning("rose.exe not found at: {Path}", gameExePath);
        return null;
    }

    /// <summary>
    /// Debug method to list all Rose Online related registry entries
    /// </summary>
    public void DebugRegistryEntries()
    {
        try
        {
            _logger.LogInformation("=== Debugging Rose Online Registry Entries ===");
            
            // Check uninstall keys
            var uninstallKeys = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var baseKey in uninstallKeys)
            {
                using var key = Registry.LocalMachine.OpenSubKey(baseKey);
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        if (subKeyName.ToLower().Contains("rose"))
                        {
                            using var subKey = key.OpenSubKey(subKeyName);
                            if (subKey != null)
                            {
                                _logger.LogInformation("Found Rose-related uninstall key: {Key}", $"{baseKey}\\{subKeyName}");
                                
                                var displayName = subKey.GetValue("DisplayName") as string;
                                var installLocation = subKey.GetValue("InstallLocation") as string;
                                
                                _logger.LogInformation("  DisplayName: {DisplayName}", displayName ?? "N/A");
                                _logger.LogInformation("  InstallLocation: {InstallLocation}", installLocation ?? "N/A");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registry debugging");
        }
    }
} 