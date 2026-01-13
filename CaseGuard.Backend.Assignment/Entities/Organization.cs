namespace CaseGuard.Backend.Assignment.Entities;

/// <summary>
/// Represents an organization (company or team) in the system.
/// Organizations can have multiple members and licenses.
/// </summary>
public class Organization
{
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the organization was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
    public ICollection<License> Licenses { get; set; } = new List<License>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}
