using Microsoft.Extensions.Logging;
using ROSE_Login_Manager.Services.Interfaces;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace ROSE_Login_Manager.Services.Implementations
{
    /// <summary>
    /// Service for managing Rose Online client patching operations
    /// </summary>
    public class RosePatcherService : IRosePatcherService
    {
        private readonly ILogger<RosePatcherService> _logger;
        private readonly IRoseClientService _roseClientService;

        public RosePatcherService(ILogger<RosePatcherService> logger, IRoseClientService roseClientService)
        {
            _logger = logger;
            _roseClientService = roseClientService;
        }

        /// <summary>
        /// Checks if updates are available for the Rose Online client
        /// </summary>
        public async Task<bool> CheckForUpdatesAsync(Action<string>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            var updaterPath = _roseClientService.GetUpdaterPath();
            if (string.IsNullOrEmpty(updaterPath))
            {
                progressCallback?.Invoke("Rose Online updater not found. Please ensure the game is properly installed.");
                return false;
            }

            try
            {
                progressCallback?.Invoke("Checking for updates...");
                
                // Use --verify to check for updates (parse output for out-of-date files)
                var arguments = $"--output \"{Path.GetDirectoryName(updaterPath)}\" --verify";
                var result = await RunUpdaterAsync(updaterPath, arguments, progressCallback, cancellationToken);
                
                // Parse the output to determine if updates are available
                return ParseUpdateCheckResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates");
                progressCallback?.Invoke($"Error checking for updates: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Patches the Rose Online client with available updates
        /// </summary>
        public async Task<bool> PatchClientAsync(Action<string>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            var updaterPath = _roseClientService.GetUpdaterPath();
            if (string.IsNullOrEmpty(updaterPath))
            {
                progressCallback?.Invoke("Rose Online updater not found. Please ensure the game is properly installed.");
                return false;
            }

            try
            {
                progressCallback?.Invoke("Starting patch process...");
                
                // Use --force-recheck to force a full patch
                var arguments = $"--output \"{Path.GetDirectoryName(updaterPath)}\" --force-recheck";
                var result = await RunUpdaterAsync(updaterPath, arguments, progressCallback, cancellationToken);
                
                return result.ExitCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching client");
                progressCallback?.Invoke($"Error patching client: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifies the integrity of Rose Online client files
        /// </summary>
        public async Task<bool> VerifyFilesAsync(Action<string>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            var updaterPath = _roseClientService.GetUpdaterPath();
            if (string.IsNullOrEmpty(updaterPath))
            {
                progressCallback?.Invoke("Rose Online updater not found. Please ensure the game is properly installed.");
                return false;
            }

            try
            {
                progressCallback?.Invoke("Verifying game files...");
                
                var arguments = $"--output \"{Path.GetDirectoryName(updaterPath)}\" --verify";
                var result = await RunUpdaterAsync(updaterPath, arguments, progressCallback, cancellationToken);
                
                return result.ExitCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying files");
                progressCallback?.Invoke($"Error verifying files: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the current version of the Rose Online client
        /// </summary>
        public async Task<string?> GetClientVersionAsync()
        {
            var updaterPath = _roseClientService.GetUpdaterPath();
            if (string.IsNullOrEmpty(updaterPath))
                return null;

            try
            {
                var arguments = "--version --quiet";
                var result = await RunUpdaterAsync(updaterPath, arguments, null, CancellationToken.None);
                
                if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
                {
                    // Parse version from output
                    return ParseVersionFromOutput(result.Output);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client version");
            }

            return null;
        }

        /// <summary>
        /// Gets the latest available version from the server
        /// </summary>
        public async Task<string?> GetLatestVersionAsync()
        {
            var updaterPath = _roseClientService.GetUpdaterPath();
            if (string.IsNullOrEmpty(updaterPath))
                return null;

            try
            {
                var arguments = "--latest-version --quiet";
                var result = await RunUpdaterAsync(updaterPath, arguments, null, CancellationToken.None);
                
                if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
                {
                    // Parse latest version from output
                    return ParseVersionFromOutput(result.Output);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest version");
            }

            return null;
        }

        /// <summary>
        /// Runs the updater executable with specified arguments
        /// </summary>
        private async Task<(int ExitCode, string Output, string Error)> RunUpdaterAsync(
            string updaterPath, 
            string arguments, 
            Action<string>? progressCallback = null, 
            CancellationToken cancellationToken = default)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = updaterPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    WorkingDirectory = Path.GetDirectoryName(updaterPath)
                }
            };

            var output = new System.Text.StringBuilder();
            var error = new System.Text.StringBuilder();
            var killed = false;

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                    progressCallback?.Invoke(e.Data);
                    _logger.LogDebug("Updater output: {Output}", e.Data);

                    if (!killed && e.Data.Contains("Ready to launch"))
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                killed = true;
                                _logger.LogInformation("Updater process killed after 'Ready to launch' detected.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error killing updater process after 'Ready to launch'.");
                        }
                    }
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    error.AppendLine(e.Data);
                    _logger.LogWarning("Updater error: {Error}", e.Data);
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                return (process.ExitCode, output.ToString(), error.ToString());
            }
            finally
            {
                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error killing updater process");
                    }
                }
            }
        }

        /// <summary>
        /// Parses the result of an update check to determine if updates are available
        /// </summary>
        private bool ParseUpdateCheckResult((int ExitCode, string Output, string Error) result)
        {
            // If the process failed, treat as error (or handle as you wish)
            if (result.ExitCode != 0)
                return false;

            // Look for the clone process count in the output
            var output = result.Output;
            var match = System.Text.RegularExpressions.Regex.Match(output, @"Starting clone process.*count\s*=\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int count))
            {
                return count > 0; // true if updates are available
            }

            // Fallback: if we can't find the pattern, assume no updates
            return false;
        }

        /// <summary>
        /// Parses version information from updater output
        /// </summary>
        private string? ParseVersionFromOutput(string output)
        {
            // Look for version patterns like "1.2.3" or "v1.2.3"
            var versionMatch = Regex.Match(output, @"(?:v?)(\d+\.\d+\.\d+)");
            return versionMatch.Success ? versionMatch.Groups[1].Value : null;
        }
    }
} 