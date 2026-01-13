namespace CaseGuard.Backend.Assignment.Contracts.Invitations.Responses;

/// <summary>
/// Response DTO for invitation information.
/// </summary>
public class InvitationResponse
{
    /// <summary>
    /// Invitation identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Organization ID.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the invited user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User ID if the user already exists in the system.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Role that will be assigned when the invitation is accepted.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Status of the invitation (Pending, Accepted, Cancelled, Expired).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Expiration date of the invitation.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the invitation is currently valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Timestamp when the invitation was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the invitation was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optional timestamp when the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// Optional timestamp when the invitation was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }
}
