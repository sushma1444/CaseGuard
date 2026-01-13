using CaseGuard.Backend.Assignment.Constants;

namespace CaseGuard.Backend.Assignment.Entities;

/// <summary>
/// Represents the relationship between a user and an organization with a specific role.
/// This is a many-to-many relationship with additional role information.
/// </summary>
public class OrganizationMember
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key to the User.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Foreign key to the Organization.
    /// </summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>
    /// Role of the user within this organization.
    /// Possible values: Owner, OrganizationAdmin, Member
    /// </summary>
    public string Role { get; set; } = Roles.Member;
    
    /// <summary>
    /// Timestamp when the user joined the organization.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the membership was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the membership was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public ICollection<LicenseAssignment> LicenseAssignments { get; set; } = new List<LicenseAssignment>();
}
