namespace CaseGuard.Backend.Assignment.Contracts.Organizations.Responses;

/// <summary>
/// Response DTO for organization information.
/// </summary>
public class OrganizationResponse
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
    /// Timestamp when the organization was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the organization was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Number of members in the organization.
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Number of active licenses for the organization.
    /// </summary>
    public int ActiveLicenseCount { get; set; }

    /// <summary>
    /// Current user's role in this organization (if applicable).
    /// </summary>
    public string? CurrentUserRole { get; set; }
}
