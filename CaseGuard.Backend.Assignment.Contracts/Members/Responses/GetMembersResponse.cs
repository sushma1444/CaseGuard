using CaseGuard.Backend.Assignment.Contracts.Common;

namespace CaseGuard.Backend.Assignment.Contracts.Members.Responses;

/// <summary>
/// Response DTO for paginated list of members.
/// </summary>
public class GetMembersResponse : PaginationResponse<MemberResponse>
{
}
