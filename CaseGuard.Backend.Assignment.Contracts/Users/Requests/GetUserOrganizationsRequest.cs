using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Users.Requests;

/// <summary>
/// Request DTO for getting a list of organizations the user belongs to.
/// </summary>
public class GetUserOrganizationsRequest : PaginationRequest
{
    /// <summary>
    /// Filter by role.
    /// </summary>
    public string? Role { get; set; }
}
