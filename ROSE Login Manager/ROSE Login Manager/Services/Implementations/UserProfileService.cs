using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ROSE_Login_Manager.Models;
using ROSE_Login_Manager.Services.Interfaces;
using ROSE_Login_Manager.Services.Models;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ROSE_Login_Manager.Services.Implementations;

/// <summary>
/// Implementation of user profile service for CRUD operations
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IDatabaseService _databaseService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IDatabaseService databaseService,
        IEncryptionService encryptionService,
        ILogger<UserProfileService> logger)
    {
        _databaseService = databaseService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    /// <summary>
    /// Adds a new user profile
    /// </summary>
    public async Task<UserProfile> AddProfileAsync(UserProfile profile)
    {
        try
        {
            // Validate profile
            var validationResult = await ValidateProfileAsync(profile);
            if (!validationResult.IsValid)
            {
                throw new ValidationException($"Profile validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Encrypt password
            profile.EncryptedGameAccountPassword = _encryptionService.EncryptString(profile.GameAccountPassword);

            // Set timestamps
            profile.CreatedAt = DateTime.UtcNow;
            profile.ModifiedAt = DateTime.UtcNow;

            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO UserProfiles (Id, ProfileName, GameAccountUsername, EncryptedGameAccountPassword, 
                                        GameClientPath, LauncherTheme, CreatedAt, ModifiedAt, IsDefault, Notes)
                VALUES (@Id, @ProfileName, @GameAccountUsername, @EncryptedGameAccountPassword, 
                       @GameClientPath, @LauncherTheme, @CreatedAt, @ModifiedAt, @IsDefault, @Notes)";

            await connection.ExecuteAsync(sql, profile);

            _logger.LogInformation("Added new user profile: {ProfileName}", profile.ProfileName);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add user profile: {ProfileName}", profile.ProfileName);
            throw;
        }
    }

    /// <summary>
    /// Gets a user profile by ID
    /// </summary>
    public async Task<UserProfile?> GetProfileAsync(Guid id)
    {
        try
        {
            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT * FROM UserProfiles WHERE Id = @Id";
            var profile = await connection.QueryFirstOrDefaultAsync<UserProfile>(sql, new { Id = id.ToString() });

            if (profile != null)
            {
                // Decrypt password
                profile.GameAccountPassword = _encryptionService.DecryptToString(profile.EncryptedGameAccountPassword);
            }

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user profile: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets all user profiles
    /// </summary>
    public async Task<IEnumerable<UserProfile>> GetAllProfilesAsync()
    {
        try
        {
            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT * FROM UserProfiles ORDER BY ProfileName";
            var profiles = await connection.QueryAsync<UserProfile>(sql);

            // Decrypt passwords for all profiles
            foreach (var profile in profiles)
            {
                profile.GameAccountPassword = _encryptionService.DecryptToString(profile.EncryptedGameAccountPassword);
            }

            return profiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all user profiles");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user profile
    /// </summary>
    public async Task<bool> UpdateProfileAsync(UserProfile profile)
    {
        try
        {
            // Validate profile
            var validationResult = await ValidateProfileAsync(profile);
            if (!validationResult.IsValid)
            {
                throw new ValidationException($"Profile validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Encrypt password if it's not already encrypted
            if (!string.IsNullOrEmpty(profile.GameAccountPassword))
            {
                profile.EncryptedGameAccountPassword = _encryptionService.EncryptString(profile.GameAccountPassword);
            }

            // Update timestamp
            profile.ModifiedAt = DateTime.UtcNow;

            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                UPDATE UserProfiles 
                SET ProfileName = @ProfileName, GameAccountUsername = @GameAccountUsername, 
                    EncryptedGameAccountPassword = @EncryptedGameAccountPassword, GameClientPath = @GameClientPath,
                    LauncherTheme = @LauncherTheme, ModifiedAt = @ModifiedAt, IsDefault = @IsDefault, Notes = @Notes
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, profile);

            _logger.LogInformation("Updated user profile: {ProfileName}", profile.ProfileName);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile: {ProfileName}", profile.ProfileName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a user profile
    /// </summary>
    public async Task<bool> DeleteProfileAsync(Guid id)
    {
        try
        {
            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            var sql = "DELETE FROM UserProfiles WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id.ToString() });

            _logger.LogInformation("Deleted user profile: {Id}", id);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user profile: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets the default profile
    /// </summary>
    public async Task<UserProfile?> GetDefaultProfileAsync()
    {
        try
        {
            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT * FROM UserProfiles WHERE IsDefault = 1 LIMIT 1";
            var profile = await connection.QueryFirstOrDefaultAsync<UserProfile>(sql);

            if (profile != null)
            {
                // Decrypt password
                profile.GameAccountPassword = _encryptionService.DecryptToString(profile.EncryptedGameAccountPassword);
            }

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get default profile");
            throw;
        }
    }

    /// <summary>
    /// Sets a profile as the default
    /// </summary>
    public async Task<bool> SetDefaultProfileAsync(Guid id)
    {
        try
        {
            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            // First, clear all default flags
            await connection.ExecuteAsync("UPDATE UserProfiles SET IsDefault = 0");

            // Then set the specified profile as default
            var sql = "UPDATE UserProfiles SET IsDefault = 1 WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id.ToString() });

            _logger.LogInformation("Set default profile: {Id}", id);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set default profile: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Validates a user profile
    /// </summary>
    public async Task<Models.ValidationResult> ValidateProfileAsync(UserProfile profile)
    {
        var result = new Models.ValidationResult { IsValid = true };

        // Basic validation
        if (string.IsNullOrWhiteSpace(profile.ProfileName))
        {
            result.Errors.Add("Profile name is required");
            result.IsValid = false;
        }

        if (string.IsNullOrWhiteSpace(profile.GameAccountUsername))
        {
            result.Errors.Add("Game account username is required");
            result.IsValid = false;
        }

        if (string.IsNullOrWhiteSpace(profile.GameAccountPassword))
        {
            result.Errors.Add("Game account password is required");
            result.IsValid = false;
        }

        if (string.IsNullOrWhiteSpace(profile.GameClientPath))
        {
            result.Errors.Add("Game client path is required");
            result.IsValid = false;
        }

        // Check if game client path exists
        if (!string.IsNullOrWhiteSpace(profile.GameClientPath) && !File.Exists(profile.GameClientPath))
        {
            result.Errors.Add("Game client path does not exist");
            result.IsValid = false;
        }

        // Check for duplicate profile names
        if (!string.IsNullOrWhiteSpace(profile.ProfileName))
        {
            var existingProfiles = await GetAllProfilesAsync();
            var duplicate = existingProfiles.FirstOrDefault(p => 
                p.ProfileName.Equals(profile.ProfileName, StringComparison.OrdinalIgnoreCase) && 
                p.Id != profile.Id);
            
            if (duplicate != null)
            {
                result.Errors.Add("A profile with this name already exists");
                result.IsValid = false;
            }
        }

        return result;
    }
} 