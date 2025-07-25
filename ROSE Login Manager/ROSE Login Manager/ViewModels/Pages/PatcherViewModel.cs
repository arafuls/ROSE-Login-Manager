using System;
using System.Windows.Media;
using System.Windows.Documents;
using ROSE_Login_Manager.Models;
using ROSE_Login_Manager.Services.Interfaces;
using Wpf.Ui.Abstractions.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace ROSE_Login_Manager.ViewModels.Pages
{
    public partial class PatcherViewModel : ViewModelBase, INavigationAware, IDisposable
    {
        private readonly IRoseClientService _roseClientService;
        private readonly IRosePatcherService _rosePatcherService;
        private bool _isInitialized = false;
        private CancellationTokenSource? _currentOperationCts;
        private readonly Regex _ansiColorRegex = new(@"\x1B\[[0-9;]*[mK]", RegexOptions.Compiled);
        private readonly Regex _timestampRegex = new(@"^\s*\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+Z\s*", RegexOptions.Compiled);
        private readonly Regex _logPrefixRegex = new(@"INFO (rose_updater|rose_update::manifest):\s*", RegexOptions.Compiled);

        public PatcherViewModel(ILogger<PatcherViewModel> logger, IRoseClientService roseClientService, IRosePatcherService rosePatcherService)
            : base(logger)
        {
            _roseClientService = roseClientService;
            _rosePatcherService = rosePatcherService;
            ParsedStatusLines = new ObservableCollection<string>();
        }

        #region Properties

        [ObservableProperty]
        private string _statusMessage = "";

        [ObservableProperty]
        private bool _isPatching = false;

        [ObservableProperty]
        private double _patchProgress = 0;

        [ObservableProperty]
        private ObservableCollection<string> _parsedStatusLines;

        // Event to notify UI when status document should be updated
        public event Action<string>? StatusLineAdded;

        #endregion

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync()
        {
            CancelCurrentOperation();
            return Task.CompletedTask;
        }

        private void InitializeViewModel()
        {
            _roseClientService.DebugRegistryEntries();

            var installPath = _roseClientService.GetClientInstallPath();
            if (!string.IsNullOrEmpty(installPath))
            {
                StatusMessage = "ROSE Online installation detected automatically.";
                _logger.LogInformation("Auto-detected ROSE Online installation at: {Path}", installPath);
            }
            else
            {
                StatusMessage = "ROSE Online installation not found. Please select the game directory manually.";
                _logger.LogWarning("ROSE Online installation not found in registry.");
            }

            _isInitialized = true;
        }

        [RelayCommand]
        private async Task CheckForUpdatesAsync()
        {
            await ExecutePatcherOperationAsync(
                async (progressCallback, cancellationToken) => await _rosePatcherService.CheckForUpdatesAsync(progressCallback, cancellationToken)
            );
        }



        private async Task ExecutePatcherOperationAsync(Func<Action<string>, CancellationToken, Task<bool>> operation)
        {
            await ExecuteAsync(async () =>
            {
                await InitializePatcherOperationAsync();
                
                var progressTracker = new ProgressTracker();
                var progressCallback = CreateProgressCallback(progressTracker);

                try
                {
                    var result = await operation(progressCallback, _currentOperationCts!.Token);
                    await FinalizePatcherOperationAsync(progressTracker);
                }
                finally
                {
                    await CleanupPatcherOperationAsync();
                }
            });
        }

        private async Task InitializePatcherOperationAsync()
        {
            CancelCurrentOperation();
            _currentOperationCts = new CancellationTokenSource();
            
            IsPatching = true;
            PatchProgress = 0;
            ParsedStatusLines.Clear();
            StatusMessage = string.Empty;
            
            await Task.CompletedTask; // For consistency with async pattern
        }

        private Action<string> CreateProgressCallback(ProgressTracker progressTracker)
        {
            return (progress) =>
            {
                progressTracker.AddProgress(progress);
                
                if (progressTracker.ShouldUpdateUI())
                {
                    UpdateUIWithProgress(progressTracker);
                }
            };
        }

        private void UpdateUIWithProgress(ProgressTracker progressTracker)
        {
            var userLines = ParseUserFriendlyLines(progressTracker.GetProgressBuffer());
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddNewLinesToUI(userLines, progressTracker);
                UpdateStatusMessage(userLines);
            });
        }

        private void AddNewLinesToUI(List<string> userLines, ProgressTracker progressTracker)
        {
            foreach (var line in userLines)
            {
                if (!progressTracker.IsLineProcessed(line))
                {
                    ParsedStatusLines.Add(line);
                    progressTracker.MarkLineAsProcessed(line);
                }
            }
        }

        private void UpdateStatusMessage(List<string> userLines)
        {
            if (userLines.Count > 0)
            {
                var lastLine = userLines.Last();
                StatusLineAdded?.Invoke(lastLine);
                StatusMessage = lastLine; // Keep StatusMessage for backward compatibility
            }
        }



        private async Task FinalizePatcherOperationAsync(ProgressTracker progressTracker)
        {
            var finalUserLines = ParseUserFriendlyLines(progressTracker.GetProgressBuffer());
            
            foreach (var line in finalUserLines)
            {
                ParsedStatusLines.Add(line);
                StatusLineAdded?.Invoke(line);
            }

            if (finalUserLines.Count > 0)
            {
                StatusMessage = finalUserLines.Last();
            }
            
            await Task.CompletedTask; // For consistency with async pattern
        }

        private async Task CleanupPatcherOperationAsync()
        {
            IsPatching = false;
            PatchProgress = 0;
            _currentOperationCts?.Dispose();
            _currentOperationCts = null;
            
            await Task.CompletedTask; // For consistency with async pattern
        }

        private void CancelCurrentOperation()
        {
            if (_currentOperationCts?.IsCancellationRequested == false)
            {
                _currentOperationCts.Cancel();
                StatusMessage = "Patching cancelled.";
                IsPatching = false;
            }
        }

        private List<string> ParseUserFriendlyLines(IEnumerable<string> outputLines)
        {
            var cleanLines = new List<string>();
            
            foreach (var line in outputLines)
            {
                var cleanLine = CleanLogLine(line);
                if (!string.IsNullOrWhiteSpace(cleanLine))
                {
                    cleanLines.Add(cleanLine);
                }
            }
            
            return cleanLines;
        }

        private string CleanLogLine(string line)
        {
            string clean = _ansiColorRegex.Replace(line, "");
            clean = _timestampRegex.Replace(clean, "");
            clean = _logPrefixRegex.Replace(clean, "");
            return clean.Trim();
        }

        public void Dispose()
        {
            CancelCurrentOperation();
            _currentOperationCts?.Dispose();
        }

        /// <summary>
        /// Helper class to track progress updates and manage UI throttling
        /// </summary>
        private class ProgressTracker
        {
            private readonly List<string> _progressBuffer = new();
            private readonly HashSet<string> _processedLines = new();
            private DateTime _lastUpdateTime = DateTime.MinValue;
            private const int UpdateIntervalMs = 100; // Throttle UI updates

            public void AddProgress(string progress)
            {
                _progressBuffer.Add(progress);
            }

            public bool ShouldUpdateUI()
            {
                var now = DateTime.UtcNow;
                var shouldUpdate = (now - _lastUpdateTime).TotalMilliseconds >= UpdateIntervalMs;
                if (shouldUpdate)
                {
                    _lastUpdateTime = now;
                }
                return shouldUpdate;
            }

            public List<string> GetProgressBuffer()
            {
                return _progressBuffer.ToList();
            }

            public bool IsLineProcessed(string line)
            {
                return _processedLines.Contains(line);
            }

            public void MarkLineAsProcessed(string line)
            {
                _processedLines.Add(line);
            }
        }
    }
}
