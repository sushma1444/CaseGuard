namespace CaseGuard.Backend.Assignment.Contracts.Licenses.Responses;

/// <summary>
/// Response DTO for license information.
/// </summary>
public class LicenseResponse
{
    /// <summary>
    /// License identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Organization ID that owns this license.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Name or description of the license.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the license.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Expiration date of the license.
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Whether auto-renewal is enabled.
    /// </summary>
    public bool AutoRenewalEnabled { get; set; }

    /// <summary>
    /// Whether the license is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether the license is currently valid (not expired and active).
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Timestamp when the license was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the license was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optional cancellation timestamp.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Number of users this license is assigned to.
    /// </summary>
    public int AssignedUserCount { get; set; }
}
