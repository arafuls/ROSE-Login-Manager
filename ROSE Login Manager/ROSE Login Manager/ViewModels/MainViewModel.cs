using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ROSE_Login_Manager.Models;
using ROSE_Login_Manager.Services.Interfaces;
using System.Collections.ObjectModel;
using System.IO;

namespace ROSE_Login_Manager.ViewModels;

/// <summary>
/// Main ViewModel for the ROSE Online Login Manager application
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILauncherSettingsService _launcherSettingsService;

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IUserProfileService userProfileService,
        ILauncherSettingsService launcherSettingsService) : base(logger)
    {
        _userProfileService = userProfileService;
        _launcherSettingsService = launcherSettingsService;
        
        Profiles = new ObservableCollection<UserProfile>();
        
        // Initialize commands
        LoadProfilesCommand = new AsyncRelayCommand(LoadProfilesAsync);
        AddProfileCommand = new AsyncRelayCommand(AddProfileAsync);
        EditProfileCommand = new AsyncRelayCommand<UserProfile>(EditProfileAsync);
        DeleteProfileCommand = new AsyncRelayCommand<UserProfile>(DeleteProfileAsync);
        LaunchGameCommand = new AsyncRelayCommand<UserProfile>(LaunchGameAsync);
        SetDefaultProfileCommand = new AsyncRelayCommand<UserProfile>(SetDefaultProfileAsync);
        RefreshProfilesCommand = new AsyncRelayCommand(LoadProfilesAsync);
    }

    /// <summary>
    /// Collection of user profiles
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<UserProfile> _profiles;

    /// <summary>
    /// Currently selected profile
    /// </summary>
    [ObservableProperty]
    private UserProfile? _selectedProfile;

    /// <summary>
    /// Search text for filtering profiles
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Application title
    /// </summary>
    [ObservableProperty]
    private string _title = "ROSE Online Login Manager";

    /// <summary>
    /// Current application version
    /// </summary>
    [ObservableProperty]
    private string _version = "1.0.0";

    /// <summary>
    /// Command to load all profiles
    /// </summary>
    public IAsyncRelayCommand LoadProfilesCommand { get; }

    /// <summary>
    /// Command to add a new profile
    /// </summary>
    public IAsyncRelayCommand AddProfileCommand { get; }

    /// <summary>
    /// Command to edit a profile
    /// </summary>
    public IAsyncRelayCommand<UserProfile> EditProfileCommand { get; }

    /// <summary>
    /// Command to delete a profile
    /// </summary>
    public IAsyncRelayCommand<UserProfile> DeleteProfileCommand { get; }

    /// <summary>
    /// Command to launch the game with a profile
    /// </summary>
    public IAsyncRelayCommand<UserProfile> LaunchGameCommand { get; }

    /// <summary>
    /// Command to set a profile as default
    /// </summary>
    public IAsyncRelayCommand<UserProfile> SetDefaultProfileCommand { get; }

    /// <summary>
    /// Command to refresh the profiles list
    /// </summary>
    public IAsyncRelayCommand RefreshProfilesCommand { get; }

    /// <summary>
    /// Loads all user profiles from the database
    /// </summary>
    private async Task LoadProfilesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var profiles = await _userProfileService.GetAllProfilesAsync();
            
            Profiles.Clear();
            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
            }

            _logger.LogInformation("Loaded {Count} profiles", Profiles.Count);
        });
    }

    /// <summary>
    /// Adds a new profile (placeholder for now)
    /// </summary>
    private async Task AddProfileAsync()
    {
        await ExecuteAsync(async () =>
        {
            // TODO: Open profile creation dialog
            _logger.LogInformation("Add profile command executed");
            
            // For now, just refresh the profiles
            await LoadProfilesAsync();
        });
    }

    /// <summary>
    /// Edits an existing profile (placeholder for now)
    /// </summary>
    private async Task EditProfileAsync(UserProfile? profile)
    {
        if (profile == null) return;

        await ExecuteAsync(async () =>
        {
            // TODO: Open profile editing dialog
            _logger.LogInformation("Edit profile command executed for: {ProfileName}", profile.ProfileName);
            
            // For now, just refresh the profiles
            await LoadProfilesAsync();
        });
    }

    /// <summary>
    /// Deletes a profile after confirmation
    /// </summary>
    private async Task DeleteProfileAsync(UserProfile? profile)
    {
        if (profile == null) return;

        await ExecuteAsync(async () =>
        {
            // TODO: Show confirmation dialog
            var success = await _userProfileService.DeleteProfileAsync(profile.Id);
            
            if (success)
            {
                Profiles.Remove(profile);
                if (SelectedProfile == profile)
                {
                    SelectedProfile = null;
                }
                
                _logger.LogInformation("Deleted profile: {ProfileName}", profile.ProfileName);
            }
            else
            {
                SetError("Failed to delete profile");
            }
        });
    }

    /// <summary>
    /// Launches the game with the specified profile
    /// </summary>
    private async Task LaunchGameAsync(UserProfile? profile)
    {
        if (profile == null) return;

        await ExecuteAsync(async () =>
        {
            // TODO: Implement game launching logic
            _logger.LogInformation("Launch game command executed for: {ProfileName}", profile.ProfileName);
            
            // Validate game client path
            if (!File.Exists(profile.GameClientPath))
            {
                SetError($"Game client not found at: {profile.GameClientPath}");
                return;
            }

            // TODO: Implement actual game launching with auto-login
            SetError("Game launching not yet implemented");
        });
    }

    /// <summary>
    /// Sets a profile as the default
    /// </summary>
    private async Task SetDefaultProfileAsync(UserProfile? profile)
    {
        if (profile == null) return;

        await ExecuteAsync(async () =>
        {
            var success = await _userProfileService.SetDefaultProfileAsync(profile.Id);
            
            if (success)
            {
                // Refresh profiles to update default status
                await LoadProfilesAsync();
                
                _logger.LogInformation("Set default profile: {ProfileName}", profile.ProfileName);
            }
            else
            {
                SetError("Failed to set default profile");
            }
        });
    }

    /// <summary>
    /// Filters profiles based on search text
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        // TODO: Implement profile filtering
        _logger.LogInformation("Search text changed: {SearchText}", value);
    }

    /// <summary>
    /// Handles profile selection change
    /// </summary>
    partial void OnSelectedProfileChanged(UserProfile? value)
    {
        if (value != null)
        {
            _logger.LogInformation("Selected profile: {ProfileName}", value.ProfileName);
        }
    }
} 