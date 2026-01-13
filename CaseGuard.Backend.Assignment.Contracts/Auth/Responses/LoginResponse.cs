namespace CaseGuard.Backend.Assignment.Contracts.Auth.Responses;

/// <summary>
/// Response DTO for successful login.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT authentication token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    public int ExpiresInMinutes { get; set; }

    /// <summary>
    /// Token type (always "Bearer" for JWT).
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}
