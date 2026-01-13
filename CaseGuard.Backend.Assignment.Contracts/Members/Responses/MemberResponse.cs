namespace CaseGuard.Backend.Assignment.Contracts.Members.Responses;

/// <summary>
/// Response DTO for organization member information.
/// </summary>
public class MemberResponse
{
    /// <summary>
    /// Membership identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's role in the organization.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the user joined the organization.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Number of licenses assigned to this member.
    /// </summary>
    public int AssignedLicenseCount { get; set; }

    /// <summary>
    /// Whether the member has any active license assignments.
    /// </summary>
    public bool HasActiveLicense { get; set; }
}
