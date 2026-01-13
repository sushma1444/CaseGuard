using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Auth.Requests;

/// <summary>
/// Request DTO for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User identifier.
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User role (Admin, Owner, OrganizationAdmin, Member).
    /// </summary>
    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}
