using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Invitations.Responses;

/// <summary>
/// Response DTO for paginated list of invitations.
/// </summary>
public class GetInvitationsResponse : PaginationResponse<InvitationResponse>
{
}
