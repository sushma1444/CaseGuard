using System.Security.Claims;
using CaseGuard.Backend.Assignment.Constants;
using CaseGuard.Backend.Assignment.Exceptions;
using CustomClaimTypes = CaseGuard.Backend.Assignment.Constants.ClaimTypes;

namespace CaseGuard.Backend.Assignment.Helpers;

/// <summary>
/// Helper methods for working with JWT claims.
/// </summary>
public static class ClaimsHelper
{
    /// <summary>
    /// Gets the user ID from the current user's claims.
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <returns>The user ID.</returns>
    /// <exception cref="UnauthorizedException">Thrown when user ID claim is not found.</exception>
    public static string GetUserId(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(CustomClaimTypes.UserId);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("User ID claim not found in token.");
        }
        return userId;
    }

    /// <summary>
    /// Gets the email from the current user's claims.
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <returns>The email address.</returns>
    /// <exception cref="UnauthorizedException">Thrown when email claim is not found.</exception>
    public static string GetEmail(ClaimsPrincipal user)
    {
        var email = user.FindFirstValue(CustomClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedException("Email claim not found in token.");
        }
        return email;
    }

    /// <summary>
    /// Gets the role from the current user's claims.
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <returns>The user's role.</returns>
    /// <exception cref="UnauthorizedException">Thrown when role claim is not found.</exception>
    public static string GetRole(ClaimsPrincipal user)
    {
        var role = user.FindFirstValue(CustomClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new UnauthorizedException("Role claim not found in token.");
        }
        return role;
    }

    /// <summary>
    /// Checks if the current user has the specified role.
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    public static bool HasRole(ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }

    /// <summary>
    /// Checks if the current user is an admin.
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <returns>True if the user is an admin, false otherwise.</returns>
    public static bool IsAdmin(ClaimsPrincipal user)
    {
        return HasRole(user, Roles.Admin);
    }

    /// <summary>
    /// Checks if the current user is an organization owner or admin.
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <returns>True if the user is an owner or organization admin, false otherwise.</returns>
    public static bool IsOwnerOrOrganizationAdmin(ClaimsPrincipal user)
    {
        return HasRole(user, Roles.Owner) || 
               HasRole(user, Roles.OrganizationAdmin) || 
               HasRole(user, Roles.Admin);
    }

    /// <summary>
    /// Gets the organization ID from the current user's claims (if present).
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <returns>The organization ID if present in claims, null otherwise.</returns>
    public static Guid? GetOrganizationId(ClaimsPrincipal user)
    {
        var organizationIdClaim = user.FindFirstValue(CustomClaimTypes.OrganizationId);
        if (string.IsNullOrWhiteSpace(organizationIdClaim) || 
            !Guid.TryParse(organizationIdClaim, out var organizationId))
        {
            return null;
        }
        return organizationId;
    }

    /// <summary>
    /// Gets the current user's ID as a Guid.
    /// </summary>
    /// <param name="user">The current user's claims principal.</param>
    /// <returns>The user ID as a Guid.</returns>
    /// <exception cref="UnauthorizedException">Thrown when user ID claim is not found or invalid.</exception>
    public static Guid GetUserIdAsGuid(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!Guid.TryParse(userId, out var userIdGuid))
        {
            throw new UnauthorizedException("Invalid user ID format in token.");
        }
        return userIdGuid;
    }
}
