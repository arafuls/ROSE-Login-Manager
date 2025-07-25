namespace ROSE_Login_Manager.Services.Interfaces;

/// <summary>
/// Service for managing ROSE Online client patching operations
/// </summary>
public interface IRosePatcherService
{
    /// <summary>
    /// Checks if updates are available for the ROSE Online client
    /// </summary>
    /// <param name="progressCallback">Callback for progress updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updates are available, false otherwise</returns>
    Task<bool> CheckForUpdatesAsync(Action<string>? progressCallback = null, CancellationToken cancellationToken = default);



    /// <summary>
    /// Gets the current version of the ROSE Online client
    /// </summary>
    /// <returns>The version string if available, null otherwise</returns>
    Task<string?> GetClientVersionAsync();

    /// <summary>
    /// Gets the latest available version from the server
    /// </summary>
    /// <returns>The version string if available, null otherwise</returns>
    Task<string?> GetLatestVersionAsync();
} 