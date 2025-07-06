namespace ROSE_Login_Manager.Services
{
    /// <summary>
    /// Service for managing Rose Online client patching operations
    /// </summary>
    public interface IRosePatcherService
    {
        /// <summary>
        /// Checks if updates are available for the Rose Online client
        /// </summary>
        /// <param name="progressCallback">Callback for progress updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if updates are available, false otherwise</returns>
        Task<bool> CheckForUpdatesAsync(Action<string>? progressCallback = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Patches the Rose Online client with available updates
        /// </summary>
        /// <param name="progressCallback">Callback for progress updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if patching was successful, false otherwise</returns>
        Task<bool> PatchClientAsync(Action<string>? progressCallback = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies the integrity of Rose Online client files
        /// </summary>
        /// <param name="progressCallback">Callback for progress updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if verification passed, false otherwise</returns>
        Task<bool> VerifyFilesAsync(Action<string>? progressCallback = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current version of the Rose Online client
        /// </summary>
        /// <returns>The version string if available, null otherwise</returns>
        Task<string?> GetClientVersionAsync();

        /// <summary>
        /// Gets the latest available version from the server
        /// </summary>
        /// <returns>The version string if available, null otherwise</returns>
        Task<string?> GetLatestVersionAsync();
    }
} 