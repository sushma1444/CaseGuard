namespace CaseGuard.Backend.Assignment.Entities;

/// <summary>
/// Represents a user in the system.
/// Users can belong to multiple organizations with different roles.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Unique email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<OrganizationMember> OrganizationMembers { get; set; } = new List<OrganizationMember>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    public ICollection<LicenseAssignment> LicenseAssignments { get; set; } = new List<LicenseAssignment>();
}
