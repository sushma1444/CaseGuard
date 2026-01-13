using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Organizations.Responses;

/// <summary>
/// Response DTO for paginated list of organizations.
/// </summary>
public class GetOrganizationsResponse : PaginationResponse<OrganizationResponse>
{
}
