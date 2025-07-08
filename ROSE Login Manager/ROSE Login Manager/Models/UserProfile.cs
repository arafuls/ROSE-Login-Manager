using System.ComponentModel.DataAnnotations;

namespace ROSE_Login_Manager.Models;

/// <summary>
/// Represents a user profile for ROSE Online game account management
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Unique identifier for the profile
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-friendly name for the profile (e.g., "Main Account", "Alt Account")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>
    /// ROSE Online account username
    /// </summary>
    [Required]
    [StringLength(50)]
    public string GameAccountUsername { get; set; } = string.Empty;

    /// <summary>
    /// Game account password (not stored in database, used for temporary operations)
    /// </summary>
    public string GameAccountPassword { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted password stored as bytes
    /// </summary>
    [Required]
    public byte[] EncryptedGameAccountPassword { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Path to the game client executable
    /// </summary>
    [Required]
    public string GameClientPath { get; set; } = string.Empty;

    /// <summary>
    /// Selected theme for this profile (optional, could be global)
    /// </summary>
    public string? LauncherTheme { get; set; }

    /// <summary>
    /// Date and time when the profile was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the profile was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this profile is the default profile
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Additional notes for the profile
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
} 