using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Users.Responses;

/// <summary>
/// Response DTO for paginated list of user organizations.
/// </summary>
public class GetUserOrganizationsResponse : PaginationResponse<UserOrganizationResponse>
{
}
