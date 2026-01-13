using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Organizations.Requests;

/// <summary>
/// Request DTO for getting a list of organizations with pagination and filtering.
/// </summary>
public class GetOrganizationsRequest : PaginationRequest
{
    /// <summary>
    /// Filter by organization name (partial match).
    /// </summary>
    public string? NameFilter { get; set; }
}
