namespace CaseGuard.Backend.Assignment.Entities;

/// <summary>
/// Represents a license (subscription) for an organization.
/// Licenses control access and features for organizations and their users.
/// </summary>
public class License
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key to the Organization that owns this license.
    /// </summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>
    /// Name or description of the license.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the license becomes active.
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the license expires.
    /// Default expiration is 10 minutes from creation for testing purposes.
    /// </summary>
    public DateTime ExpirationDate { get; set; }
    
    /// <summary>
    /// Indicates whether the license should automatically renew before expiration.
    /// </summary>
    public bool AutoRenewalEnabled { get; set; }
    
    /// <summary>
    /// Indicates whether the license is currently active and valid.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Timestamp when the license was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the license was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Optional cancellation timestamp if the license was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public ICollection<LicenseAssignment> LicenseAssignments { get; set; } = new List<LicenseAssignment>();
    
    /// <summary>
    /// Checks if the license is currently valid (not expired and active).
    /// </summary>
    public bool IsValid => IsActive && ExpirationDate > DateTime.UtcNow && CancelledAt == null;
}
