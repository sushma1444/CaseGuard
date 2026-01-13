namespace CaseGuard.Backend.Assignment.Contracts.Auth.Responses;

/// <summary>
/// Response DTO containing JWT token claims.
/// </summary>
public class ClaimsResponse
{
    /// <summary>
    /// User identifier from token.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User email from token.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User role from token.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// All claims from the JWT token.
    /// </summary>
    public Dictionary<string, string> AllClaims { get; set; } = new();
}
