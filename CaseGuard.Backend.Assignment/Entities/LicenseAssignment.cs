namespace CaseGuard.Backend.Assignment.Entities;

/// <summary>
/// Represents the assignment of a license to a specific user within an organization.
/// This allows organization owners/admins to assign licenses to specific members.
/// </summary>
public class LicenseAssignment
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key to the License.
    /// </summary>
    public Guid LicenseId { get; set; }
    
    /// <summary>
    /// Foreign key to the User.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Foreign key to the OrganizationMember (for easier querying).
    /// </summary>
    public Guid OrganizationMemberId { get; set; }
    
    /// <summary>
    /// Timestamp when the license was assigned to the user.
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the assignment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Optional timestamp when the license was unassigned from the user.
    /// </summary>
    public DateTime? UnassignedAt { get; set; }

    // Navigation properties
    public License License { get; set; } = null!;
    public User User { get; set; } = null!;
    public OrganizationMember OrganizationMember { get; set; } = null!;
    
    /// <summary>
    /// Checks if the assignment is currently active.
    /// </summary>
    public bool IsActive => UnassignedAt == null;
}
