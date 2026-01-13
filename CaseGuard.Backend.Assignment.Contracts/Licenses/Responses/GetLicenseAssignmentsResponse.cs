using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Licenses.Responses;

/// <summary>
/// Response DTO for paginated list of license assignments.
/// </summary>
public class GetLicenseAssignmentsResponse : PaginationResponse<LicenseAssignmentResponse>
{
}
