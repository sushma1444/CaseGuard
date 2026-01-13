using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Members.Requests;

/// <summary>
/// Request DTO for inviting a user to join an organization.
/// </summary>
public class InviteMemberRequest
{
    /// <summary>
    /// Email address of the user to invite.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Role to assign to the user when they accept the invitation.
    /// </summary>
    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = "Member";

    /// <summary>
    /// Optional expiration date for the invitation. Defaults to 7 days from now.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
