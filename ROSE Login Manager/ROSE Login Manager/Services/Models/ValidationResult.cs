namespace ROSE_Login_Manager.Services.Models;

/// <summary>
/// Result of profile validation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation was successful
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
} 