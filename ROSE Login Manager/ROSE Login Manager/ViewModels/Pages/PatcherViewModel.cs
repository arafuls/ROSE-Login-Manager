using System.Windows.Media;
using ROSE_Login_Manager.Models;
using ROSE_Login_Manager.Services;
using Wpf.Ui.Abstractions.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ROSE_Login_Manager.ViewModels.Pages
{
    public partial class PatcherViewModel : ObservableObject, INavigationAware
    {
        private readonly ILogger<PatcherViewModel> _logger;
        private readonly IRoseClientService _roseClientService;
        private readonly IRosePatcherService _rosePatcherService;
        private bool _isInitialized = false;

        public PatcherViewModel(ILogger<PatcherViewModel> logger, IRoseClientService roseClientService, IRosePatcherService rosePatcherService)
        {
            _logger = logger;
            _roseClientService = roseClientService;
            _rosePatcherService = rosePatcherService;
            ParsedStatusLines = new ObservableCollection<string>();
        }

        [ObservableProperty]
        private string _gameClientPath = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "Ready to check for updates";

        [ObservableProperty]
        private bool _isPatching = false;

        [ObservableProperty]
        private double _patchProgress = 0;

        [ObservableProperty]
        private string _lastUpdateCheck = "Last checked: Never";

        [ObservableProperty]
        private string _statusIndicatorText = "READY";

        [ObservableProperty]
        private Brush _statusIndicatorBrush = new SolidColorBrush(Colors.LimeGreen);

        [ObservableProperty]
        private ObservableCollection<string> _parsedStatusLines;

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            // Debug registry entries to help with development
            _roseClientService.DebugRegistryEntries();

            // Auto-detect Rose Online installation
            var installPath = _roseClientService.GetClientInstallPath();
            if (!string.IsNullOrEmpty(installPath))
            {
                GameClientPath = installPath;
                StatusMessage = "Rose Online installation detected automatically";
                _logger.LogInformation("Auto-detected Rose Online installation at: {Path}", installPath);
            }
            else
            {
                StatusMessage = "Rose Online installation not found. Please select the game directory manually.";
                _logger.LogWarning("Rose Online installation not found in registry");
            }

            _isInitialized = true;
        }

        [RelayCommand]
        private void BrowseClientPath()
        {
            var openFolderDialog = new OpenFolderDialog
            {
                Title = "Select ROSE Online Game Installation Directory",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Multiselect = false
            };

            bool? result = openFolderDialog.ShowDialog();
            if (result == true)
            {
                GameClientPath = openFolderDialog.FolderName;
                // Optional: Update a UI element, e.g., a TextBox
                // myPathTextBox.Text = GameClientPath;
            }
        }

        [RelayCommand]
        private async Task CheckForUpdatesAsync()
        {
            if (string.IsNullOrEmpty(GameClientPath))
            {
                StatusMessage = "Please select a game client path first";
                return;
            }

            IsPatching = true;
            StatusMessage = "Checking for updates...";
            PatchProgress = 0;
            ParsedStatusLines.Clear();
            SetStatusIndicator(true);

            try
            {
                string patcherOutput = string.Empty;
                var hasUpdates = await _rosePatcherService.CheckForUpdatesAsync(
                    progress =>
                    {
                        patcherOutput += progress + "\n";

                        var userLines = ParseUserFriendlyLines(patcherOutput);
                        ParsedStatusLines.Clear();
                        foreach (var line in userLines)
                            ParsedStatusLines.Add(line);

                        // Set StatusMessage to the last important line, or a default
                        if (userLines.Count > 0)
                            StatusMessage += '\n' + userLines.Last();
                    });


                SetStatusIndicator(hasUpdates);

                LastUpdateCheck = $"Last checked: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates");
                StatusMessage = $"Error checking for updates: {ex.Message}";
                SetStatusIndicator(false);
            }
            finally
            {
                IsPatching = false;
                PatchProgress = 0;
            }
        }

        [RelayCommand]
        private async Task PatchGameAsync()
        {
            if (string.IsNullOrEmpty(GameClientPath))
            {
                StatusMessage = "Please select a game client path first";
                return;
            }

            IsPatching = true;
            StatusMessage = "Patching game client...";
            PatchProgress = 0;
            ParsedStatusLines.Clear();
            SetStatusIndicator(true);

            try
            {
                string patcherOutput = string.Empty;
                var success = await _rosePatcherService.PatchClientAsync(
                    progress =>
                    {
                        patcherOutput += progress + "\n";
                    });

                var userLines = ParseUserFriendlyLines(patcherOutput);
                ParsedStatusLines.Clear();
                foreach (var line in userLines)
                    ParsedStatusLines.Add(line);

                if (userLines.Count > 0)
                    StatusMessage = userLines.Last();
                else
                    StatusMessage = "No status available.";

                SetStatusIndicator(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching game client");
                StatusMessage = $"Error patching game: {ex.Message}";
                SetStatusIndicator(false);
            }
            finally
            {
                IsPatching = false;
                PatchProgress = 0;
            }
        }

        [RelayCommand]
        private async Task VerifyFilesAsync()
        {
            if (string.IsNullOrEmpty(GameClientPath))
            {
                StatusMessage = "Please select a game client path first";
                return;
            }

            IsPatching = true;
            StatusMessage = "Verifying game files...";
            PatchProgress = 0;
            ParsedStatusLines.Clear();
            SetStatusIndicator(true);

            try
            {
                string patcherOutput = string.Empty;
                var success = await _rosePatcherService.VerifyFilesAsync(
                    progress =>
                    {
                        patcherOutput += progress + "\n";
                    });

                var userLines = ParseUserFriendlyLines(patcherOutput);
                ParsedStatusLines.Clear();
                foreach (var line in userLines)
                    ParsedStatusLines.Add(line);

                if (userLines.Count > 0)
                    StatusMessage = userLines.Last();
                else
                    StatusMessage = "No status available.";

                SetStatusIndicator(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying files");
                StatusMessage = $"Error verifying files: {ex.Message}";
                SetStatusIndicator(false);
            }
            finally
            {
                IsPatching = false;
                PatchProgress = 0;
            }
        }

        [RelayCommand]
        private async Task RefreshStatusAsync()
        {
            StatusMessage = "Refreshing status...";
            await Task.Delay(1000);
            StatusMessage = "Ready to check for updates";
            ParsedStatusLines.Clear();
            SetStatusIndicator(true);
        }

        private void SetStatusIndicator(bool ready)
        {
            StatusIndicatorText = ready ? "READY" : "";
            StatusIndicatorBrush = new SolidColorBrush(Colors.LimeGreen);
        }

        private List<string> ParseUserFriendlyLines(string output)
        {
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var importantLines = new List<string>();
            foreach (var line in lines)
            {
                // Remove ANSI color codes and timestamps
                var clean = Regex.Replace(line, @"\x1B\[[0-9;]*[mK]", ""); // Remove ANSI
                clean = Regex.Replace(clean, @"^\s*\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+Z\s*", ""); // Remove timestamp
                clean = Regex.Replace(clean, @"INFO (rose_updater|rose_update::manifest):\s*", ""); // Remove log prefix

                if (clean.Contains("Building local chunk indexes") ||
                    clean.Contains("Initializing clone outputs") ||
                    clean.Contains("Downloading missing chunks") ||
                    clean.Contains("Saving local manifest") ||
                    clean.Contains("Download task completed") ||
                    clean.Contains("Application updated") ||
                    clean.Contains("Ready to launch"))
                {
                    importantLines.Add(clean.Trim());
                }
            }
            return importantLines;
        }
    }
}
