using System.Security.Claims;
using CaseGuard.Backend.Assignment.Constants;
using CaseGuard.Backend.Assignment.Data;
using CaseGuard.Backend.Assignment.Entities;
using CaseGuard.Backend.Assignment.Exceptions;
using CustomClaimTypes = CaseGuard.Backend.Assignment.Constants.ClaimTypes;
using Microsoft.EntityFrameworkCore;

namespace CaseGuard.Backend.Assignment.Helpers;

/// <summary>
/// Helper methods for authorization and organization membership checks.
/// </summary>
public static class AuthorizationHelper
{
    /// <summary>
    /// Checks if a user is a member of a specific organization.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="organizationId">The organization ID to check.</param>
    /// <returns>True if the user is a member, false otherwise.</returns>
    public static async Task<bool> IsMemberOfOrganizationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId)
    {
        return await dbContext.OrganizationMembers
            .AnyAsync(om => om.UserId == userId && om.OrganizationId == organizationId);
    }

    /// <summary>
    /// Gets the organization membership for a user in a specific organization.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="organizationId">The organization ID.</param>
    /// <returns>The organization membership if found, null otherwise.</returns>
    public static async Task<OrganizationMember?> GetOrganizationMembershipAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId)
    {
        return await dbContext.OrganizationMembers
            .FirstOrDefaultAsync(om => om.UserId == userId && om.OrganizationId == organizationId);
    }

    /// <summary>
    /// Checks if a user has a specific role in an organization.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="organizationId">The organization ID to check.</param>
    /// <param name="requiredRole">The required role (Owner, OrganizationAdmin, or Member).</param>
    /// <returns>True if the user has the required role or higher, false otherwise.</returns>
    public static async Task<bool> HasRoleInOrganizationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId,
        string requiredRole)
    {
        var membership = await GetOrganizationMembershipAsync(dbContext, userId, organizationId);
        if (membership == null)
        {
            return false;
        }

        // Admin users have access to all organizations
        // Check if user is system admin (this would be in their JWT claims)
        // For now, we'll check the membership role

        return requiredRole switch
        {
            Roles.Member => membership.Role == Roles.Member || 
                           membership.Role == Roles.OrganizationAdmin || 
                           membership.Role == Roles.Owner,
            Roles.OrganizationAdmin => membership.Role == Roles.OrganizationAdmin || 
                                      membership.Role == Roles.Owner,
            Roles.Owner => membership.Role == Roles.Owner,
            _ => false
        };
    }

    /// <summary>
    /// Checks if a user is an owner or admin of an organization.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="organizationId">The organization ID to check.</param>
    /// <returns>True if the user is owner or admin, false otherwise.</returns>
    public static async Task<bool> IsOwnerOrAdminOfOrganizationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId)
    {
        var membership = await GetOrganizationMembershipAsync(dbContext, userId, organizationId);
        if (membership == null)
        {
            return false;
        }

        return membership.Role == Roles.Owner || membership.Role == Roles.OrganizationAdmin;
    }

    /// <summary>
    /// Checks if a user is the owner of an organization.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="organizationId">The organization ID to check.</param>
    /// <returns>True if the user is the owner, false otherwise.</returns>
    public static async Task<bool> IsOwnerOfOrganizationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId)
    {
        var membership = await GetOrganizationMembershipAsync(dbContext, userId, organizationId);
        return membership?.Role == Roles.Owner;
    }

    /// <summary>
    /// Verifies that a user is a member of an organization and throws an exception if not.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to verify.</param>
    /// <param name="organizationId">The organization ID to verify.</param>
    /// <exception cref="ForbiddenException">Thrown when the user is not a member of the organization.</exception>
    public static async Task EnsureUserIsMemberOfOrganizationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId)
    {
        var isMember = await IsMemberOfOrganizationAsync(dbContext, userId, organizationId);
        if (!isMember)
        {
            throw new ForbiddenException("You do not have access to this organization.");
        }
    }

    /// <summary>
    /// Verifies that a user has a specific role in an organization and throws an exception if not.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to verify.</param>
    /// <param name="organizationId">The organization ID to verify.</param>
    /// <param name="requiredRole">The required role.</param>
    /// <exception cref="ForbiddenException">Thrown when the user does not have the required role.</exception>
    public static async Task EnsureUserHasRoleInOrganizationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId,
        string requiredRole)
    {
        var hasRole = await HasRoleInOrganizationAsync(dbContext, userId, organizationId, requiredRole);
        if (!hasRole)
        {
            throw new ForbiddenException($"You do not have the required role ({requiredRole}) in this organization.");
        }
    }

    /// <summary>
    /// Verifies that a user is an owner or admin of an organization and throws an exception if not.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to verify.</param>
    /// <param name="organizationId">The organization ID to verify.</param>
    /// <exception cref="ForbiddenException">Thrown when the user is not an owner or admin.</exception>
    public static async Task EnsureUserIsOwnerOrAdminOfOrganizationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId)
    {
        var isOwnerOrAdmin = await IsOwnerOrAdminOfOrganizationAsync(dbContext, userId, organizationId);
        if (!isOwnerOrAdmin)
        {
            throw new ForbiddenException("You must be an owner or admin of this organization to perform this action.");
        }
    }

    /// <summary>
    /// Verifies that a user is the owner of an organization and throws an exception if not.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to verify.</param>
    /// <param name="organizationId">The organization ID to verify.</param>
    /// <exception cref="ForbiddenException">Thrown when the user is not the owner.</exception>
    public static async Task EnsureUserIsOwnerOfOrganizationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid organizationId)
    {
        var isOwner = await IsOwnerOfOrganizationAsync(dbContext, userId, organizationId);
        if (!isOwner)
        {
            throw new ForbiddenException("You must be the owner of this organization to perform this action.");
        }
    }

    /// <summary>
    /// Checks if a user can access a license (must be a member of the license's organization).
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="licenseId">The license ID to check.</param>
    /// <returns>True if the user can access the license, false otherwise.</returns>
    public static async Task<bool> CanAccessLicenseAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid licenseId)
    {
        var license = await dbContext.Licenses
            .FirstOrDefaultAsync(l => l.Id == licenseId);

        if (license == null)
        {
            return false;
        }

        return await IsMemberOfOrganizationAsync(dbContext, userId, license.OrganizationId);
    }

    /// <summary>
    /// Checks if a user can access a license assignment (must be a member of the assignment's organization).
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="assignmentId">The license assignment ID to check.</param>
    /// <returns>True if the user can access the assignment, false otherwise.</returns>
    public static async Task<bool> CanAccessLicenseAssignmentAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid assignmentId)
    {
        var assignment = await dbContext.LicenseAssignments
            .Include(la => la.License)
            .FirstOrDefaultAsync(la => la.Id == assignmentId);

        if (assignment?.License == null)
        {
            return false;
        }

        return await IsMemberOfOrganizationAsync(dbContext, userId, assignment.License.OrganizationId);
    }

    /// <summary>
    /// Checks if a user can access an invitation (must be a member of the invitation's organization).
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="invitationId">The invitation ID to check.</param>
    /// <returns>True if the user can access the invitation, false otherwise.</returns>
    public static async Task<bool> CanAccessInvitationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid invitationId)
    {
        var invitation = await dbContext.Invitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
        {
            return false;
        }

        return await IsMemberOfOrganizationAsync(dbContext, userId, invitation.OrganizationId);
    }

    /// <summary>
    /// Gets the organization ID from the current user's claims (if present).
    /// This is useful when a user is scoped to a single organization.
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <returns>The organization ID if present in claims, null otherwise.</returns>
    public static Guid? GetOrganizationIdFromClaims(ClaimsPrincipal user)
    {
        var organizationIdClaim = user.FindFirstValue(CustomClaimTypes.OrganizationId);
        if (string.IsNullOrWhiteSpace(organizationIdClaim) || 
            !Guid.TryParse(organizationIdClaim, out var organizationId))
        {
            return null;
        }
        return organizationId;
    }
}
