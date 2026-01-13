namespace CaseGuard.Backend.Assignment.Contracts.Users.Responses;

/// <summary>
/// Response DTO for accepting an invitation.
/// </summary>
public class AcceptInvitationResponse
{
    /// <summary>
    /// Organization ID that the user joined.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Role assigned to the user.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the invitation was accepted.
    /// </summary>
    public DateTime AcceptedAt { get; set; }
}
