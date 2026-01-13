using CaseGuard.Backend.Assignment.Constants;

namespace CaseGuard.Backend.Assignment.Entities;

/// <summary>
/// Represents an invitation for a user to join an organization.
/// Invitations are sent via email and can be accepted or cancelled.
/// </summary>
public class Invitation
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key to the Organization.
    /// </summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>
    /// Email address of the user being invited.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Foreign key to the User if they already exist in the system (optional).
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Role that will be assigned to the user when they accept the invitation.
    /// </summary>
    public string Role { get; set; } = Roles.Member;
    
    /// <summary>
    /// Status of the invitation: Pending, Accepted, Cancelled, Expired
    /// </summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    
    /// <summary>
    /// Timestamp when the invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Timestamp when the invitation was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the invitation was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Optional timestamp when the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
    
    /// <summary>
    /// Optional timestamp when the invitation was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public User? User { get; set; }
    
    /// <summary>
    /// Checks if the invitation is currently valid (pending and not expired).
    /// </summary>
    public bool IsValid => Status == InvitationStatus.Pending && ExpiresAt > DateTime.UtcNow;
}

/// <summary>
/// Status of an invitation.
/// </summary>
public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Cancelled = 2,
    Expired = 3
}
