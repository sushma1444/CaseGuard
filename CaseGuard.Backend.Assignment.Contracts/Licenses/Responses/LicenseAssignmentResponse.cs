namespace CaseGuard.Backend.Assignment.Contracts.Licenses.Responses;

/// <summary>
/// Response DTO for license assignment information.
/// </summary>
public class LicenseAssignmentResponse
{
    /// <summary>
    /// Assignment identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// License ID.
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// License name.
    /// </summary>
    public string LicenseName { get; set; } = string.Empty;

    /// <summary>
    /// User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User email.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// User name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Organization ID.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the license was assigned.
    /// </summary>
    public DateTime AssignedAt { get; set; }

    /// <summary>
    /// Whether the assignment is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Optional timestamp when the license was unassigned.
    /// </summary>
    public DateTime? UnassignedAt { get; set; }

    /// <summary>
    /// License expiration date.
    /// </summary>
    public DateTime LicenseExpirationDate { get; set; }

    /// <summary>
    /// Whether the license is valid (not expired).
    /// </summary>
    public bool LicenseIsValid { get; set; }
}
