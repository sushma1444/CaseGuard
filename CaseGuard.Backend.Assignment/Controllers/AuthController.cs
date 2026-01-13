using CaseGuard.Backend.Assignment.Contracts.Auth.Requests;
using CaseGuard.Backend.Assignment.Contracts.Auth.Responses;
using CaseGuard.Backend.Assignment.Exceptions;
using CaseGuard.Backend.Assignment.Helpers;
using CaseGuard.Backend.Assignment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseGuard.Backend.Assignment.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IJwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login request containing user credentials.</param>
    /// <returns>JWT token response.</returns>
    /// <response code="200">Returns the JWT token.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Validate role
            var validRoles = new[] { "Admin", "Owner", "OrganizationAdmin", "Member" };
            if (!validRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            {
                throw new BadRequestException($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(request.UserId, request.Email, request.Role);
            var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);

            _logger.LogInformation("User {UserId} ({Email}) logged in with role {Role}", 
                request.UserId, request.Email, request.Role);

            var response = new LoginResponse
            {
                Token = token,
                ExpiresInMinutes = expirationMinutes,
                TokenType = "Bearer"
            };

            return Ok(response);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {UserId}", request.UserId);
            throw new BadRequestException("An error occurred during login. Please try again.");
        }
    }

    /// <summary>
    /// Returns the claims from the current user's JWT token.
    /// </summary>
    /// <returns>Claims from the JWT token.</returns>
    /// <response code="200">Returns the claims.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("claims")]
    [Authorize]
    [ProducesResponseType(typeof(ClaimsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetClaims()
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            var email = ClaimsHelper.GetEmail(User);
            var role = ClaimsHelper.GetRole(User);

            // Get all claims
            var allClaims = User.Claims
                .ToDictionary(c => c.Type, c => c.Value);

            var response = new ClaimsResponse
            {
                UserId = userId,
                Email = email,
                Role = role,
                AllClaims = allClaims
            };

            return Ok(response);
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving claims for user");
            throw new UnauthorizedException("Unable to retrieve user claims.");
        }
    }
}
