using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Licenses.Requests;

/// <summary>
/// Request DTO for getting license assignments with pagination and filtering.
/// </summary>
public class GetLicenseAssignmentsRequest : PaginationRequest
{
    /// <summary>
    /// Filter by license ID.
    /// </summary>
    public Guid? LicenseId { get; set; }

    /// <summary>
    /// Filter by user ID.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Filter by organization ID.
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// Filter by active status (only active assignments).
    /// </summary>
    public bool? ActiveOnly { get; set; }
}
