using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Members.Requests;

/// <summary>
/// Request DTO for updating a member's role in an organization.
/// </summary>
public class UpdateMemberRoleRequest
{
    /// <summary>
    /// New role for the member.
    /// </summary>
    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}
