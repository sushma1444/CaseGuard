using CaseGuard.Backend.Assignment.Contracts.Licenses.Requests;
using CaseGuard.Backend.Assignment.Contracts.Licenses.Responses;
using CaseGuard.Backend.Assignment.Data;
using CaseGuard.Backend.Assignment.Entities;
using CaseGuard.Backend.Assignment.Exceptions;
using CaseGuard.Backend.Assignment.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaseGuard.Backend.Assignment.Controllers;

/// <summary>
/// Controller for license assignment management operations.
/// Organization Owners and Admins can assign/unassign licenses to members.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LicenseAssignmentController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LicenseAssignmentController> _logger;

    public LicenseAssignmentController(
        ApplicationDbContext dbContext,
        ILogger<LicenseAssignmentController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Assigns a license to a user in an organization.
    /// </summary>
    /// <param name="request">Assignment request with license ID and user ID.</param>
    /// <returns>Created assignment information.</returns>
    /// <response code="201">License assigned successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the license or user is not found.</response>
    /// <response code="403">If the user does not have permission to assign licenses.</response>
    /// <response code="409">If the license is already assigned to the user.</response>
    [HttpPost]
    [ProducesResponseType(typeof(LicenseAssignmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignLicense([FromBody] AssignLicenseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Get license with organization
            var license = await _dbContext.Licenses
                .Include(l => l.Organization)
                .FirstOrDefaultAsync(l => l.Id == request.LicenseId);

            if (license == null)
            {
                throw new NotFoundException("License", request.LicenseId);
            }

            // Check authorization: Admins can assign any license, others must be Owner or OrganizationAdmin of the license's organization
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, license.OrganizationId);
            }

            // Validate license is valid and active
            if (!license.IsValid)
            {
                throw new BadRequestException("Cannot assign an invalid or expired license.");
            }

            // Get user
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null)
            {
                throw new NotFoundException("User", request.UserId);
            }

            // Verify user is a member of the license's organization
            var organizationMember = await _dbContext.OrganizationMembers
                .FirstOrDefaultAsync(om => om.UserId == request.UserId && om.OrganizationId == license.OrganizationId);

            if (organizationMember == null)
            {
                throw new BadRequestException("User must be a member of the license's organization before assigning a license.");
            }

            // Check if license is already assigned to this user (active assignment)
            var existingAssignment = await _dbContext.LicenseAssignments
                .FirstOrDefaultAsync(la => 
                    la.LicenseId == request.LicenseId && 
                    la.UserId == request.UserId && 
                    la.UnassignedAt == null);

            if (existingAssignment != null)
            {
                throw new BadRequestException("License is already assigned to this user.");
            }

            // Create assignment
            var assignment = new LicenseAssignment
            {
                Id = Guid.NewGuid(),
                LicenseId = request.LicenseId,
                UserId = request.UserId,
                OrganizationMemberId = organizationMember.Id,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.LicenseAssignments.Add(assignment);
            await _dbContext.SaveChangesAsync();

            // Load related entities for response
            await _dbContext.Entry(assignment)
                .Reference(a => a.License)
                .LoadAsync();
            await _dbContext.Entry(assignment.License)
                .Reference(l => l.Organization)
                .LoadAsync();
            await _dbContext.Entry(assignment)
                .Reference(a => a.User)
                .LoadAsync();
            await _dbContext.Entry(assignment)
                .Reference(a => a.OrganizationMember)
                .LoadAsync();
            await _dbContext.Entry(assignment.OrganizationMember)
                .Reference(om => om.Organization)
                .LoadAsync();

            var response = MapToLicenseAssignmentResponse(assignment);

            _logger.LogInformation("License {LicenseId} assigned to user {UserId} in organization {OrganizationId} by user {AssignedBy}",
                request.LicenseId, request.UserId, license.OrganizationId, CurrentUserId);

            return CreatedAtAction(nameof(GetLicenseAssignment), new { id = assignment.Id }, response);
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
            _logger.LogError(ex, "Error assigning license {LicenseId} to user {UserId}", request.LicenseId, request.UserId);
            throw new BadRequestException("Failed to assign license. Please check your input and try again.");
        }
    }

    /// <summary>
    /// Gets a paginated list of license assignments with optional filtering and sorting.
    /// </summary>
    /// <param name="request">Pagination and filtering parameters.</param>
    /// <returns>Paginated list of license assignments.</returns>
    /// <response code="200">Returns the list of license assignments.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="403">If the user does not have access to view assignments.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetLicenseAssignmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLicenseAssignments([FromQuery] GetLicenseAssignmentsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Build query
            var query = _dbContext.LicenseAssignments
                .Include(la => la.License)
                    .ThenInclude(l => l.Organization)
                .Include(la => la.User)
                .Include(la => la.OrganizationMember)
                .AsQueryable();

            // Apply filters
            if (request.LicenseId.HasValue)
            {
                query = query.Where(la => la.LicenseId == request.LicenseId.Value);
            }

            if (request.UserId.HasValue)
            {
                query = query.Where(la => la.UserId == request.UserId.Value);
            }

            if (request.OrganizationId.HasValue)
            {
                query = query.Where(la => la.License.OrganizationId == request.OrganizationId.Value);
            }

            if (request.ActiveOnly == true)
            {
                query = query.Where(la => la.UnassignedAt == null);
            }

            // Authorization: Admins can see all, others see only their organization's assignments
            if (!IsAdmin)
            {
                // Get user's organization IDs
                var userOrganizationIds = await _dbContext.OrganizationMembers
                    .Where(om => om.UserId == CurrentUserIdGuid)
                    .Select(om => om.OrganizationId)
                    .ToListAsync();

                query = query.Where(la => userOrganizationIds.Contains(la.License.OrganizationId));
            }

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(la =>
                    la.License.Name.ToLower().Contains(searchTerm) ||
                    la.User.Email.ToLower().Contains(searchTerm) ||
                    la.User.Name.ToLower().Contains(searchTerm) ||
                    la.License.Organization.Name.ToLower().Contains(searchTerm));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                var sortDirection = request.SortDirection?.ToLower() == "desc" ? "desc" : "asc";
                query = request.SortBy.ToLower() switch
                {
                    "assignedat" => sortDirection == "desc"
                        ? query.OrderByDescending(la => la.AssignedAt)
                        : query.OrderBy(la => la.AssignedAt),
                    "unassignedat" => sortDirection == "desc"
                        ? query.OrderByDescending(la => la.UnassignedAt ?? DateTime.MaxValue)
                        : query.OrderBy(la => la.UnassignedAt ?? DateTime.MaxValue),
                    "useremail" => sortDirection == "desc"
                        ? query.OrderByDescending(la => la.User.Email)
                        : query.OrderBy(la => la.User.Email),
                    "licensename" => sortDirection == "desc"
                        ? query.OrderByDescending(la => la.License.Name)
                        : query.OrderBy(la => la.License.Name),
                    "organizationname" => sortDirection == "desc"
                        ? query.OrderByDescending(la => la.License.Organization.Name)
                        : query.OrderBy(la => la.License.Organization.Name),
                    _ => query.OrderByDescending(la => la.AssignedAt) // Default sort
                };
            }
            else
            {
                // Default sort by assignment date (newest first)
                query = query.OrderByDescending(la => la.AssignedAt);
            }

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            var assignments = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to response DTOs
            var assignmentResponses = assignments.Select(assignment =>
                MapToLicenseAssignmentResponse(assignment)
            ).ToList();

            var response = new GetLicenseAssignmentsResponse
            {
                Items = assignmentResponses,
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving license assignments");
            throw new BadRequestException("Failed to retrieve license assignments. Please check your query parameters and try again.");
        }
    }

    /// <summary>
    /// Gets a specific license assignment by ID.
    /// </summary>
    /// <param name="id">License assignment ID.</param>
    /// <returns>License assignment information.</returns>
    /// <response code="200">Returns the license assignment.</response>
    /// <response code="404">If the assignment is not found.</response>
    /// <response code="403">If the user does not have access to this assignment.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LicenseAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLicenseAssignment(Guid id)
    {
        try
        {
            var assignment = await _dbContext.LicenseAssignments
                .Include(la => la.License)
                    .ThenInclude(l => l.Organization)
                .Include(la => la.User)
                .Include(la => la.OrganizationMember)
                .FirstOrDefaultAsync(la => la.Id == id);

            if (assignment == null)
            {
                throw new NotFoundException("LicenseAssignment", id);
            }

            // Check authorization: Admins can access any assignment, others must be members of the organization
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsMemberOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, assignment.License.OrganizationId);
            }

            var response = MapToLicenseAssignmentResponse(assignment);

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
            _logger.LogError(ex, "Error retrieving license assignment {AssignmentId}", id);
            throw new NotFoundException("LicenseAssignment", id);
        }
    }

    /// <summary>
    /// Unassigns a license from a user.
    /// </summary>
    /// <param name="id">License assignment ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">License unassigned successfully.</response>
    /// <response code="404">If the assignment is not found.</response>
    /// <response code="403">If the user does not have permission to unassign licenses.</response>
    /// <response code="400">If the assignment is already unassigned.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnassignLicense(Guid id)
    {
        try
        {
            var assignment = await _dbContext.LicenseAssignments
                .Include(la => la.License)
                .FirstOrDefaultAsync(la => la.Id == id);

            if (assignment == null)
            {
                throw new NotFoundException("LicenseAssignment", id);
            }

            // Check authorization: Admins can unassign any license, others must be Owner or OrganizationAdmin of the license's organization
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, assignment.License.OrganizationId);
            }

            // Check if already unassigned
            if (assignment.UnassignedAt != null)
            {
                throw new BadRequestException("License assignment is already unassigned.");
            }

            // Unassign the license
            assignment.UnassignedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("License assignment {AssignmentId} unassigned by user {UserId}",
                id, CurrentUserId);

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
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning license assignment {AssignmentId}", id);
            throw new NotFoundException("LicenseAssignment", id);
        }
    }

    /// <summary>
    /// Maps a LicenseAssignment entity to a LicenseAssignmentResponse DTO.
    /// </summary>
    private static LicenseAssignmentResponse MapToLicenseAssignmentResponse(LicenseAssignment assignment)
    {
        return new LicenseAssignmentResponse
        {
            Id = assignment.Id,
            LicenseId = assignment.LicenseId,
            LicenseName = assignment.License?.Name ?? string.Empty,
            UserId = assignment.UserId,
            UserEmail = assignment.User?.Email ?? string.Empty,
            UserName = assignment.User?.Name ?? string.Empty,
            OrganizationId = assignment.License?.OrganizationId ?? Guid.Empty,
            OrganizationName = assignment.License?.Organization?.Name ?? string.Empty,
            AssignedAt = assignment.AssignedAt,
            IsActive = assignment.IsActive,
            UnassignedAt = assignment.UnassignedAt,
            LicenseExpirationDate = assignment.License?.ExpirationDate ?? DateTime.MinValue,
            LicenseIsValid = assignment.License?.IsValid ?? false
        };
    }
}
