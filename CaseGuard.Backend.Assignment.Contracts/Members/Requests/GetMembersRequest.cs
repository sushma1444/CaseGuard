using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Members.Requests;

/// <summary>
/// Request DTO for getting a list of members with pagination and filtering.
/// </summary>
public class GetMembersRequest : PaginationRequest
{
    /// <summary>
    /// Filter by role.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Filter by email (partial match).
    /// </summary>
    public string? EmailFilter { get; set; }
}
