using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Invitations.Requests;

/// <summary>
/// Request DTO for getting a list of invitations with pagination and filtering.
/// </summary>
public class GetInvitationsRequest : PaginationRequest
{
    /// <summary>
    /// Filter by invitation status (Pending, Accepted, Cancelled, Expired).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by email (partial match).
    /// </summary>
    public string? EmailFilter { get; set; }
}
