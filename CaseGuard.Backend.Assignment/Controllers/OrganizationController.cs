using CaseGuard.Backend.Assignment.Contracts.Organizations.Requests;
using CaseGuard.Backend.Assignment.Contracts.Organizations.Responses;
using CaseGuard.Backend.Assignment.Data;
using CaseGuard.Backend.Assignment.Entities;
using CaseGuard.Backend.Assignment.Exceptions;
using CaseGuard.Backend.Assignment.Helpers;
using CaseGuard.Backend.Assignment.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaseGuard.Backend.Assignment.Controllers;

/// <summary>
/// Controller for organization management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<OrganizationController> _logger;

    public OrganizationController(
        ApplicationDbContext dbContext,
        ILogger<OrganizationController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new organization. The creator becomes the Owner of the organization.
    /// </summary>
    /// <param name="request">Organization creation request.</param>
    /// <returns>Created organization information.</returns>
    /// <response code="201">Organization created successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verify user exists before creating organization
            var userExists = await _dbContext.Users
                .AnyAsync(u => u.Id == CurrentUserIdGuid);

            if (!userExists)
            {
                throw new BadRequestException("User not found. Please ensure you are properly authenticated.");
            }

            // Check if organization name already exists
            var existingOrganization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Name.ToLower() == request.Name.ToLower());

            if (existingOrganization != null)
            {
                throw new BadRequestException("An organization with this name already exists.");
            }

            // Create organization
            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Organizations.Add(organization);

            // Create organization membership for the creator as Owner
            var membership = new OrganizationMember
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUserIdGuid,
                OrganizationId = organization.Id,
                Role = Roles.Owner,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.OrganizationMembers.Add(membership);

            await _dbContext.SaveChangesAsync();

            // Get member count and active license count
            var memberCount = await _dbContext.OrganizationMembers
                .CountAsync(om => om.OrganizationId == organization.Id);

            var activeLicenseCount = await _dbContext.Licenses
                .CountAsync(l => l.OrganizationId == organization.Id && l.IsActive && l.IsValid);

            var response = MapToOrganizationResponse(organization, memberCount, activeLicenseCount, Roles.Owner);

            _logger.LogInformation("Organization {OrganizationId} created by user {UserId}", 
                organization.Id, CurrentUserId);

            return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, response);
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
            _logger.LogError(ex, "Error creating organization");
            throw new BadRequestException("Failed to create organization. Please check your input and try again.");
        }
    }

    /// <summary>
    /// Gets a paginated list of organizations with optional filtering and sorting.
    /// Admins can see all organizations. Regular users see only organizations they are members of.
    /// </summary>
    /// <param name="request">Pagination and filtering parameters.</param>
    /// <returns>Paginated list of organizations.</returns>
    /// <response code="200">Returns the list of organizations.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetOrganizationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrganizations([FromQuery] GetOrganizationsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            IQueryable<Organization> query;

            // Admins can see all organizations
            if (IsAdmin)
            {
                query = _dbContext.Organizations.AsQueryable();
            }
            else
            {
                // Regular users see only organizations they are members of
                var userOrganizationIds = await _dbContext.OrganizationMembers
                    .Where(om => om.UserId == CurrentUserIdGuid)
                    .Select(om => om.OrganizationId)
                    .ToListAsync();

                // Handle empty list to avoid EF Core issues with Contains on empty collections
                if (userOrganizationIds.Count == 0)
                {
                    // Return empty query result
                    query = _dbContext.Organizations.Where(o => false);
                }
                else
                {
                    query = _dbContext.Organizations
                        .Where(o => userOrganizationIds.Contains(o.Id));
                }
            }

            // Apply name filter if provided
            if (!string.IsNullOrWhiteSpace(request.NameFilter))
            {
                var nameFilter = request.NameFilter.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(nameFilter));
            }

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(o =>
                    o.Name.ToLower().Contains(searchTerm) ||
                    (o.Description != null && o.Description.ToLower().Contains(searchTerm)));
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
                        ? query.OrderByDescending(o => o.Name)
                        : query.OrderBy(o => o.Name),
                    "createdat" => sortDirection == "desc"
                        ? query.OrderByDescending(o => o.CreatedAt)
                        : query.OrderBy(o => o.CreatedAt),
                    "updatedat" => sortDirection == "desc"
                        ? query.OrderByDescending(o => o.UpdatedAt)
                        : query.OrderBy(o => o.UpdatedAt),
                    _ => query.OrderByDescending(o => o.CreatedAt) // Default sort
                };
            }
            else
            {
                // Default sort by creation date (newest first)
                query = query.OrderByDescending(o => o.CreatedAt);
            }

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            var organizations = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            // Get member counts and active license counts for each organization
            var organizationIds = organizations.Select(o => o.Id).ToList();

            Dictionary<Guid, int> memberCounts;
            Dictionary<Guid, int> activeLicenseCounts;
            Dictionary<Guid, string> userMemberships;

            if (organizationIds.Count == 0)
            {
                // If no organizations, return empty dictionaries
                memberCounts = new Dictionary<Guid, int>();
                activeLicenseCounts = new Dictionary<Guid, int>();
                userMemberships = new Dictionary<Guid, string>();
            }
            else
            {
                memberCounts = await _dbContext.OrganizationMembers
                    .Where(om => organizationIds.Contains(om.OrganizationId))
                    .GroupBy(om => om.OrganizationId)
                    .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.OrganizationId, x => x.Count);

                activeLicenseCounts = await _dbContext.Licenses
                    .Where(l => organizationIds.Contains(l.OrganizationId) && l.IsActive && l.IsValid)
                    .GroupBy(l => l.OrganizationId)
                    .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.OrganizationId, x => x.Count);

                // Get user's role in each organization
                userMemberships = await _dbContext.OrganizationMembers
                    .Where(om => om.UserId == CurrentUserIdGuid && organizationIds.Contains(om.OrganizationId))
                    .ToDictionaryAsync(om => om.OrganizationId, om => om.Role);
            }

            // Map to response DTOs
            var organizationResponses = organizations.Select(org =>
            {
                memberCounts.TryGetValue(org.Id, out var memberCount);
                activeLicenseCounts.TryGetValue(org.Id, out var activeLicenseCount);
                userMemberships.TryGetValue(org.Id, out var userRole);
                return MapToOrganizationResponse(org, memberCount, activeLicenseCount, userRole);
            }).ToList();

            var response = new GetOrganizationsResponse
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
            _logger.LogError(ex, "Error retrieving organizations");
            throw new BadRequestException("Failed to retrieve organizations. Please check your query parameters and try again.");
        }
    }

    /// <summary>
    /// Gets a specific organization by ID.
    /// Users can only view organizations they are members of (unless Admin).
    /// </summary>
    /// <param name="id">Organization ID.</param>
    /// <returns>Organization information.</returns>
    /// <response code="200">Returns the organization.</response>
    /// <response code="404">If the organization is not found.</response>
    /// <response code="403">If the user does not have access to this organization.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrganization(Guid id)
    {
        try
        {
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null)
            {
                throw new NotFoundException("Organization", id);
            }

            // Check authorization: Admins can access any organization, others must be members
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsMemberOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, id);
            }

            // Get member count and active license count
            var memberCount = await _dbContext.OrganizationMembers
                .CountAsync(om => om.OrganizationId == organization.Id);

            var activeLicenseCount = await _dbContext.Licenses
                .CountAsync(l => l.OrganizationId == organization.Id && l.IsActive && l.IsValid);

            // Get user's role in the organization
            string? userRole = null;
            if (!IsAdmin)
            {
                var membership = await _dbContext.OrganizationMembers
                    .FirstOrDefaultAsync(om => om.UserId == CurrentUserIdGuid && om.OrganizationId == id);
                userRole = membership?.Role;
            }

            var response = MapToOrganizationResponse(organization, memberCount, activeLicenseCount, userRole);

            return Ok(response);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ForbiddenException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization {OrganizationId}", id);
            throw new NotFoundException("Organization", id);
        }
    }

    /// <summary>
    /// Updates an organization's properties.
    /// Only Owners, OrganizationAdmins, or Admins can update organizations.
    /// </summary>
    /// <param name="id">Organization ID.</param>
    /// <param name="request">Update request with properties to modify.</param>
    /// <returns>Updated organization information.</returns>
    /// <response code="200">Organization updated successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the organization is not found.</response>
    /// <response code="403">If the user does not have permission to update this organization.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] UpdateOrganizationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null)
            {
                throw new NotFoundException("Organization", id);
            }

            // Check authorization: Admins can update any organization, others must be Owner or OrganizationAdmin
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, id);
            }

            // Check if new name conflicts with existing organization
            if (!string.IsNullOrWhiteSpace(request.Name) && 
                request.Name.ToLower() != organization.Name.ToLower())
            {
                var existingOrganization = await _dbContext.Organizations
                    .FirstOrDefaultAsync(o => o.Name.ToLower() == request.Name.ToLower() && o.Id != id);

                if (existingOrganization != null)
                {
                    throw new BadRequestException("An organization with this name already exists.");
                }
            }

            // Update properties if provided
            if (request.Name != null)
            {
                organization.Name = request.Name;
            }

            if (request.Description != null)
            {
                organization.Description = request.Description;
            }

            organization.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Get member count and active license count
            var memberCount = await _dbContext.OrganizationMembers
                .CountAsync(om => om.OrganizationId == organization.Id);

            var activeLicenseCount = await _dbContext.Licenses
                .CountAsync(l => l.OrganizationId == organization.Id && l.IsActive && l.IsValid);

            // Get user's role in the organization
            string? userRole = null;
            if (!IsAdmin)
            {
                var membership = await _dbContext.OrganizationMembers
                    .FirstOrDefaultAsync(om => om.UserId == CurrentUserIdGuid && om.OrganizationId == id);
                userRole = membership?.Role;
            }

            var response = MapToOrganizationResponse(organization, memberCount, activeLicenseCount, userRole);

            _logger.LogInformation("Organization {OrganizationId} updated by user {UserId}", id, CurrentUserId);

            return Ok(response);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ForbiddenException)
        {
            throw;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId}", id);
            throw new BadRequestException("Failed to update organization. Please check your input and try again.");
        }
    }

    /// <summary>
    /// Deletes an organization.
    /// Only Owners or Admins can delete organizations.
    /// </summary>
    /// <param name="id">Organization ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Organization deleted successfully.</response>
    /// <response code="404">If the organization is not found.</response>
    /// <response code="403">If the user does not have permission to delete this organization.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        try
        {
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null)
            {
                throw new NotFoundException("Organization", id);
            }

            // Check authorization: Admins can delete any organization, others must be Owner
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, id);
            }

            // Delete related entities (cascade delete should handle this, but we'll be explicit)
            // Note: In a production system, you might want to soft delete or archive instead

            // Delete all memberships
            var memberships = await _dbContext.OrganizationMembers
                .Where(om => om.OrganizationId == id)
                .ToListAsync();
            _dbContext.OrganizationMembers.RemoveRange(memberships);

            // Delete all invitations
            var invitations = await _dbContext.Invitations
                .Where(i => i.OrganizationId == id)
                .ToListAsync();
            _dbContext.Invitations.RemoveRange(invitations);

            // Delete all license assignments (licenses themselves might be kept for audit, but assignments should be removed)
            var licenseAssignments = await _dbContext.LicenseAssignments
                .Include(la => la.License)
                .Where(la => la.License.OrganizationId == id)
                .ToListAsync();
            _dbContext.LicenseAssignments.RemoveRange(licenseAssignments);

            // Cancel all licenses (set IsActive = false instead of deleting for audit trail)
            var licenses = await _dbContext.Licenses
                .Where(l => l.OrganizationId == id)
                .ToListAsync();
            foreach (var license in licenses)
            {
                license.IsActive = false;
                license.CancelledAt = DateTime.UtcNow;
                license.UpdatedAt = DateTime.UtcNow;
            }

            // Delete the organization
            _dbContext.Organizations.Remove(organization);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Organization {OrganizationId} deleted by user {UserId}", id, CurrentUserId);

            return NoContent();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ForbiddenException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {OrganizationId}", id);
            throw new NotFoundException("Organization", id);
        }
    }

    /// <summary>
    /// Maps an Organization entity to an OrganizationResponse DTO.
    /// </summary>
    private static OrganizationResponse MapToOrganizationResponse(
        Organization organization, 
        int memberCount, 
        int activeLicenseCount,
        string? currentUserRole)
    {
        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt,
            MemberCount = memberCount,
            ActiveLicenseCount = activeLicenseCount,
            CurrentUserRole = currentUserRole
        };
    }
}
