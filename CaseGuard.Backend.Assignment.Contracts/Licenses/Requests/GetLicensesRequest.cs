using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Licenses.Requests;

/// <summary>
/// Request DTO for getting a list of licenses with pagination and filtering.
/// </summary>
public class GetLicensesRequest : PaginationRequest
{
    /// <summary>
    /// Filter by organization ID.
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// Filter by active status.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by auto-renewal enabled status.
    /// </summary>
    public bool? AutoRenewalEnabled { get; set; }

    /// <summary>
    /// Filter by expiration status (expired, active, or all).
    /// </summary>
    public string? ExpirationStatus { get; set; } // "expired", "active", "all"
}
