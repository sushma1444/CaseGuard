namespace CaseGuard.Backend.Assignment.Contracts.Users.Responses;

/// <summary>
/// Response DTO for organization information from a user's perspective.
/// </summary>
public class UserOrganizationResponse
{
    /// <summary>
    /// Organization identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the organization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the organization.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// User's role in this organization.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the user joined the organization.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Timestamp when the organization was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Number of members in the organization.
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Number of active licenses for the organization.
    /// </summary>
    public int ActiveLicenseCount { get; set; }

    /// <summary>
    /// Number of licenses assigned to the current user.
    /// </summary>
    public int UserAssignedLicenseCount { get; set; }
}
