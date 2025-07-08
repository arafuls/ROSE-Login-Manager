using ROSE_Login_Manager.Models;
using ROSE_Login_Manager.Services.Models;

namespace ROSE_Login_Manager.Services.Interfaces;

/// <summary>
/// Service for managing user profile CRUD operations
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Adds a new user profile
    /// </summary>
    /// <param name="profile">The profile to add</param>
    /// <returns>The added profile with generated ID</returns>
    Task<UserProfile> AddProfileAsync(UserProfile profile);

    /// <summary>
    /// Gets a user profile by ID
    /// </summary>
    /// <param name="id">The profile ID</param>
    /// <returns>The profile or null if not found</returns>
    Task<UserProfile?> GetProfileAsync(Guid id);

    /// <summary>
    /// Gets all user profiles
    /// </summary>
    /// <returns>Collection of all profiles</returns>
    Task<IEnumerable<UserProfile>> GetAllProfilesAsync();

    /// <summary>
    /// Updates an existing user profile
    /// </summary>
    /// <param name="profile">The profile to update</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateProfileAsync(UserProfile profile);

    /// <summary>
    /// Deletes a user profile
    /// </summary>
    /// <param name="id">The profile ID to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteProfileAsync(Guid id);

    /// <summary>
    /// Gets the default profile
    /// </summary>
    /// <returns>The default profile or null if none exists</returns>
    Task<UserProfile?> GetDefaultProfileAsync();

    /// <summary>
    /// Sets a profile as the default
    /// </summary>
    /// <param name="id">The profile ID to set as default</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> SetDefaultProfileAsync(Guid id);

    /// <summary>
    /// Validates a user profile
    /// </summary>
    /// <param name="profile">The profile to validate</param>
    /// <returns>Validation result</returns>
    Task<ROSE_Login_Manager.Services.Models.ValidationResult> ValidateProfileAsync(UserProfile profile);
} 