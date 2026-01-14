using CaseGuard.Backend.Assignment.Contracts.Users.Requests;
using CaseGuard.Backend.Assignment.Contracts.Users.Responses;
using CaseGuard.Backend.Assignment.Constants;
using CaseGuard.Backend.Assignment.Data;
using CaseGuard.Backend.Assignment.Entities;
using CaseGuard.Backend.Assignment.Exceptions;
using CaseGuard.Backend.Assignment.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaseGuard.Backend.Assignment.Controllers;

/// <summary>
/// Controller for user-related operations.
/// Users can view their organizations, accept invitations, and leave organizations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserController> _logger;

    public UserController(
        ApplicationDbContext dbContext,
        ILogger<UserController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of organizations the current user belongs to.
    /// </summary>
    /// <param name="request">Pagination and filtering parameters.</param>
    /// <returns>Paginated list of user's organizations.</returns>
    /// <response code="200">Returns the list of organizations.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("organizations")]
    [ProducesResponseType(typeof(GetUserOrganizationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserOrganizations([FromQuery] GetUserOrganizationsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Get user ID first - this will throw UnauthorizedException if claims are invalid
            Guid currentUserId;
            try
            {
                currentUserId = CurrentUserIdGuid;
            }
            catch (UnauthorizedException)
            {
                throw; // Re-throw to return 401
            }
            
            // Get user's organization memberships
            var query = _dbContext.OrganizationMembers
                .Include(om => om.Organization)
                .Where(om => om.UserId == currentUserId)
                .AsQueryable();

            // Apply role filter if provided
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                query = query.Where(om => om.Role == request.Role);
            }

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(om =>
                    om.Organization.Name.ToLower().Contains(searchTerm) ||
                    (om.Organization.Description != null && om.Organization.Description.ToLower().Contains(searchTerm)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                var sortDirection = request.SortDirection?.ToLower() == "desc" ? "desc" : "asc";
                query = request.SortBy.ToLower() switch
                {
                    "name" => sortDirection == "desc"
                        ? query.OrderByDescending(om => om.Organization.Name)
                        : query.OrderBy(om => om.Organization.Name),
                    "joinedat" => sortDirection == "desc"
                        ? query.OrderByDescending(om => om.JoinedAt)
                        : query.OrderBy(om => om.JoinedAt),
                    "role" => sortDirection == "desc"
                        ? query.OrderByDescending(om => om.Role)
                        : query.OrderBy(om => om.Role),
                    _ => query.OrderByDescending(om => om.JoinedAt) // Default sort
                };
            }
            else
            {
                // Default sort by join date (newest first)
                query = query.OrderByDescending(om => om.JoinedAt);
            }

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            var memberships = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            // Get organization IDs
            var organizationIds = memberships.Select(om => om.OrganizationId).ToList();

            // Get member counts for each organization
            Dictionary<Guid, int> memberCounts = new Dictionary<Guid, int>();
            Dictionary<Guid, int> activeLicenseCounts = new Dictionary<Guid, int>();
            Dictionary<Guid, int> userLicenseCounts = new Dictionary<Guid, int>();

            if (organizationIds.Count > 0)
            {
                memberCounts = await _dbContext.OrganizationMembers
                    .Where(om => organizationIds.Contains(om.OrganizationId))
                    .GroupBy(om => om.OrganizationId)
                    .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.OrganizationId, x => x.Count);

                // Get active license counts for each organization
                activeLicenseCounts = await _dbContext.Licenses
                    .Where(l => organizationIds.Contains(l.OrganizationId) && l.IsActive && l.IsValid)
                    .GroupBy(l => l.OrganizationId)
                    .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.OrganizationId, x => x.Count);

                // Get user's assigned license counts for each organization
                var currentUserIdForLicenses = CurrentUserIdGuid;
                userLicenseCounts = await _dbContext.LicenseAssignments
                    .Include(la => la.License)
                    .Where(la => la.UserId == currentUserIdForLicenses && 
                                la.UnassignedAt == null &&
                                organizationIds.Contains(la.License.OrganizationId))
                    .GroupBy(la => la.License.OrganizationId)
                    .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.OrganizationId, x => x.Count);
            }

            // Map to response DTOs
            var organizationResponses = memberships.Select(membership =>
            {
                memberCounts.TryGetValue(membership.OrganizationId, out var memberCount);
                activeLicenseCounts.TryGetValue(membership.OrganizationId, out var activeLicenseCount);
                userLicenseCounts.TryGetValue(membership.OrganizationId, out var userLicenseCount);
                return MapToUserOrganizationResponse(membership, memberCount, activeLicenseCount, userLicenseCount);
            }).ToList();

            var response = new GetUserOrganizationsResponse
            {
                Items = organizationResponses,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            return Ok(response);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user organizations");
            throw new BadRequestException("Failed to retrieve organizations. Please check your query parameters and try again.");
        }
    }

    /// <summary>
    /// Gets details of a specific organization the current user belongs to.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <returns>Organization details.</returns>
    /// <response code="200">Returns the organization details.</response>
    /// <response code="404">If the organization is not found or user is not a member.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("organizations/{organizationId}")]
    [ProducesResponseType(typeof(UserOrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserOrganization(Guid organizationId)
    {
        try
        {
            // Get user's membership in this organization
            var membership = await _dbContext.OrganizationMembers
                .Include(om => om.Organization)
                .FirstOrDefaultAsync(om => om.UserId == CurrentUserIdGuid && om.OrganizationId == organizationId);

            if (membership == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Get member count
            var memberCount = await _dbContext.OrganizationMembers
                .CountAsync(om => om.OrganizationId == organizationId);

            // Get active license count
            var activeLicenseCount = await _dbContext.Licenses
                .CountAsync(l => l.OrganizationId == organizationId && l.IsActive && l.IsValid);

            // Get user's assigned license count
            var userLicenseCount = await _dbContext.LicenseAssignments
                .Include(la => la.License)
                .CountAsync(la => la.UserId == CurrentUserIdGuid && 
                                 la.UnassignedAt == null &&
                                 la.License.OrganizationId == organizationId);

            var response = MapToUserOrganizationResponse(membership, memberCount, activeLicenseCount, userLicenseCount);

            return Ok(response);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization {OrganizationId} for user {UserId}", organizationId, CurrentUserId);
            throw new NotFoundException("Organization", organizationId);
        }
    }

    /// <summary>
    /// Accepts an invitation to join an organization.
    /// </summary>
    /// <param name="request">Accept invitation request.</param>
    /// <returns>Acceptance confirmation with organization details.</returns>
    /// <response code="200">Invitation accepted successfully.</response>
    /// <response code="400">If the request is invalid or invitation cannot be accepted.</response>
    /// <response code="404">If the invitation is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("invitations/accept")]
    [ProducesResponseType(typeof(AcceptInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Get invitation with organization
            var invitation = await _dbContext.Invitations
                .Include(i => i.Organization)
                .FirstOrDefaultAsync(i => i.Id == request.InvitationId);

            if (invitation == null)
            {
                throw new NotFoundException("Invitation", request.InvitationId);
            }

            // Verify invitation is for this user's email
            if (invitation.Email.ToLower() != CurrentUserEmail.ToLower())
            {
                throw new BadRequestException("This invitation is not for your email address.");
            }

            // Check if invitation is valid (pending and not expired)
            if (!invitation.IsValid)
            {
                if (invitation.Status != InvitationStatus.Pending)
                {
                    throw new BadRequestException($"Invitation is not pending. Current status: {invitation.Status}.");
                }
                if (invitation.ExpiresAt <= DateTime.UtcNow)
                {
                    // Mark as expired
                    invitation.Status = InvitationStatus.Expired;
                    invitation.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                    throw new BadRequestException("This invitation has expired.");
                }
            }

            // Check if user is already a member of this organization
            var existingMembership = await _dbContext.OrganizationMembers
                .FirstOrDefaultAsync(om => om.UserId == CurrentUserIdGuid && om.OrganizationId == invitation.OrganizationId);

            if (existingMembership != null)
            {
                throw new BadRequestException("You are already a member of this organization.");
            }

            // Get or create user
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == CurrentUserIdGuid);

            if (user == null)
            {
                // Create user if doesn't exist
                user = new User
                {
                    Id = CurrentUserIdGuid,
                    Email = CurrentUserEmail,
                    Name = CurrentUserEmail.Split('@')[0], // Default name from email
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.Users.Add(user);
            }
            else
            {
                // Update user email if different
                if (user.Email.ToLower() != CurrentUserEmail.ToLower())
                {
                    user.Email = CurrentUserEmail;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Create organization membership
            var membership = new OrganizationMember
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUserIdGuid,
                OrganizationId = invitation.OrganizationId,
                Role = invitation.Role,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.OrganizationMembers.Add(membership);

            // Update invitation status
            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.UserId = CurrentUserIdGuid;
            invitation.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var response = new AcceptInvitationResponse
            {
                OrganizationId = invitation.OrganizationId,
                OrganizationName = invitation.Organization.Name,
                Role = invitation.Role,
                AcceptedAt = invitation.AcceptedAt.Value
            };

            _logger.LogInformation("User {UserId} accepted invitation {InvitationId} to join organization {OrganizationId}",
                CurrentUserId, request.InvitationId, invitation.OrganizationId);

            return Ok(response);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation {InvitationId} for user {UserId}", request.InvitationId, CurrentUserId);
            throw new BadRequestException("Failed to accept invitation. Please check your input and try again.");
        }
    }

    /// <summary>
    /// Leaves an organization the current user is a member of.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">User left the organization successfully.</response>
    /// <response code="404">If the organization is not found or user is not a member.</response>
    /// <response code="400">If the user is the Owner and cannot leave.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpDelete("organizations/{organizationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LeaveOrganization(Guid organizationId)
    {
        try
        {
            // Get user's membership
            var membership = await _dbContext.OrganizationMembers
                .Include(om => om.Organization)
                .FirstOrDefaultAsync(om => om.UserId == CurrentUserIdGuid && om.OrganizationId == organizationId);

            if (membership == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Prevent Owner from leaving (they must transfer ownership or delete organization)
            if (membership.Role == Roles.Owner)
            {
                throw new BadRequestException("Organization Owner cannot leave the organization. Please transfer ownership or delete the organization instead.");
            }

            // Unassign all license assignments for this user in this organization
            var licenseAssignments = await _dbContext.LicenseAssignments
                .Include(la => la.License)
                .Where(la => la.UserId == CurrentUserIdGuid && 
                            la.License.OrganizationId == organizationId &&
                            la.UnassignedAt == null)
                .ToListAsync();

            foreach (var assignment in licenseAssignments)
            {
                assignment.UnassignedAt = DateTime.UtcNow;
            }

            // Remove the membership
            _dbContext.OrganizationMembers.Remove(membership);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {UserId} left organization {OrganizationId}", CurrentUserId, organizationId);

            return NoContent();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving organization {OrganizationId} for user {UserId}", organizationId, CurrentUserId);
            throw new NotFoundException("Organization", organizationId);
        }
    }

    /// <summary>
    /// Maps an OrganizationMember entity to a UserOrganizationResponse DTO.
    /// </summary>
    private static UserOrganizationResponse MapToUserOrganizationResponse(
        OrganizationMember membership,
        int memberCount,
        int activeLicenseCount,
        int userAssignedLicenseCount)
    {
        return new UserOrganizationResponse
        {
            Id = membership.OrganizationId,
            Name = membership.Organization.Name,
            Description = membership.Organization.Description,
            Role = membership.Role,
            JoinedAt = membership.JoinedAt,
            CreatedAt = membership.Organization.CreatedAt,
            MemberCount = memberCount,
            ActiveLicenseCount = activeLicenseCount,
            UserAssignedLicenseCount = userAssignedLicenseCount
        };
    }
}
