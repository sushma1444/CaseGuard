namespace CaseGuard.Backend.Assignment.Contracts.Members.Responses;

/// <summary>
/// Response DTO for member invitation.
/// </summary>
public class InviteMemberResponse
{
    /// <summary>
    /// Invitation identifier.
    /// </summary>
    public Guid InvitationId { get; set; }

    /// <summary>
    /// Email address of the invited user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Role that will be assigned when the invitation is accepted.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Expiration date of the invitation.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Timestamp when the invitation was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
