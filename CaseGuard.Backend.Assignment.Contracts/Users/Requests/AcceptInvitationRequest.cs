using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Users.Requests;

/// <summary>
/// Request DTO for accepting an invitation to join an organization.
/// </summary>
public class AcceptInvitationRequest
{
    /// <summary>
    /// Invitation ID to accept.
    /// </summary>
    [Required(ErrorMessage = "Invitation ID is required.")]
    public Guid InvitationId { get; set; }
}
