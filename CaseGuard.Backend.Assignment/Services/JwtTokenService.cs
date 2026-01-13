using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CaseGuard.Backend.Assignment.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CustomClaimTypes = CaseGuard.Backend.Assignment.Constants.ClaimTypes;

namespace CaseGuard.Backend.Assignment.Services;

/// <summary>
/// Service for generating and managing JWT tokens.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        _issuer = jwtSettings["Issuer"] ?? "CaseGuard";
        _audience = jwtSettings["Audience"] ?? "CaseGuard";
        _expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes", 60);
    }

    /// <inheritdoc />
    public string GenerateToken(string userId, string email, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(CustomClaimTypes.UserId, userId),
            new Claim(CustomClaimTypes.Email, email),
            new Claim(CustomClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return GenerateToken(claims);
    }

    /// <inheritdoc />
    public string GenerateToken(IEnumerable<Claim> claims)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
