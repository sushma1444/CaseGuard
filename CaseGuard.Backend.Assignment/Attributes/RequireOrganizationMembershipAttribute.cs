using Microsoft.AspNetCore.Authorization;

namespace CaseGuard.Backend.Assignment.Attributes;

/// <summary>
/// Authorization attribute that requires the user to be a member of the organization.
/// This attribute should be used in conjunction with organization membership checks in the controller action.
/// Note: This attribute only checks for authentication. Actual organization membership must be verified
/// in the controller using AuthorizationHelper methods.
/// </summary>
public class RequireOrganizationMembershipAttribute : AuthorizeAttribute
{
    public RequireOrganizationMembershipAttribute()
    {
        Policy = "Member"; // Require authenticated user
    }
}

/// <summary>
/// Authorization attribute that requires the user to be an owner or admin of the organization.
/// This attribute checks for the role in JWT claims. Actual organization membership must be verified
/// in the controller using AuthorizationHelper methods.
/// </summary>
public class RequireOrganizationOwnerOrAdminAttribute : AuthorizeAttribute
{
    public RequireOrganizationOwnerOrAdminAttribute()
    {
        Policy = "OrganizationOwnerOrAdmin"; // Require Owner, OrganizationAdmin, or Admin role
    }
}
