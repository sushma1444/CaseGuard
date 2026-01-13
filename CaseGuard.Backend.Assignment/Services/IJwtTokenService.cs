using System.Security.Claims;

namespace CaseGuard.Backend.Assignment.Services;

/// <summary>
/// Service for generating and managing JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user claims.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="email">User email.</param>
    /// <param name="role">User role.</param>
    /// <returns>JWT token string.</returns>
    string GenerateToken(string userId, string email, string role);

    /// <summary>
    /// Generates a JWT token from a collection of claims.
    /// </summary>
    /// <param name="claims">Collection of claims to include in the token.</param>
    /// <returns>JWT token string.</returns>
    string GenerateToken(IEnumerable<Claim> claims);
}
