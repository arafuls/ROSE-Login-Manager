using System.Windows.Media;
using ROSE_Login_Manager.Models;
using Wpf.Ui.Abstractions.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;

namespace ROSE_Login_Manager.ViewModels.Pages
{
    public partial class PatcherViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

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

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            _isInitialized = true;
        }

        [RelayCommand]
        private void BrowseClientPath()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Rose Online Game Client",
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                FileName = "rose.exe"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                GameClientPath = Path.GetDirectoryName(openFileDialog.FileName) ?? string.Empty;
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

            try
            {
                // Simulate checking for updates
                await Task.Delay(2000);
                
                // Simulate finding updates
                PatchProgress = 50;
                StatusMessage = "Found 3 updates available";
                
                await Task.Delay(1000);
                PatchProgress = 100;
                StatusMessage = "Update check completed";
                LastUpdateCheck = $"Last checked: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error checking for updates: {ex.Message}";
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

            try
            {
                // Simulate patching process
                for (int i = 0; i <= 100; i += 10)
                {
                    PatchProgress = i;
                    StatusMessage = $"Patching... {i}% complete";
                    await Task.Delay(500);
                }
                
                StatusMessage = "Game client patched successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error patching game: {ex.Message}";
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

            try
            {
                // Simulate file verification
                for (int i = 0; i <= 100; i += 20)
                {
                    PatchProgress = i;
                    StatusMessage = $"Verifying files... {i}% complete";
                    await Task.Delay(300);
                }
                
                StatusMessage = "All game files verified successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error verifying files: {ex.Message}";
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
        }
    }
}
