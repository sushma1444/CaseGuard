using CaseGuard.Backend.Assignment.Contracts.Members.Requests;
using CaseGuard.Backend.Assignment.Contracts.Members.Responses;
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
/// Controller for organization member management operations.
/// Only Owners and OrganizationAdmins can manage members.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MemberController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MemberController> _logger;

    public MemberController(
        ApplicationDbContext dbContext,
        ILogger<MemberController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Invites a user to join an organization via email.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="request">Invitation request.</param>
    /// <returns>Created invitation information.</returns>
    /// <response code="201">Invitation created successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the organization is not found.</response>
    /// <response code="403">If the user does not have permission to invite members.</response>
    /// <response code="409">If the user is already a member or has a pending invitation.</response>
    [HttpPost("{organizationId}/invite")]
    [ProducesResponseType(typeof(InviteMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InviteMember(Guid organizationId, [FromBody] InviteMemberRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verify organization exists
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Check authorization: Admins can invite to any organization, others must be Owner or OrganizationAdmin
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, organizationId);
            }

            // Check if user is already a member
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                var existingMembership = await _dbContext.OrganizationMembers
                    .FirstOrDefaultAsync(om => om.UserId == existingUser.Id && om.OrganizationId == organizationId);

                if (existingMembership != null)
                {
                    throw new BadRequestException("User is already a member of this organization.");
                }
            }

            // Check if there's a pending invitation for this email
            var existingInvitation = await _dbContext.Invitations
                .FirstOrDefaultAsync(i => 
                    i.OrganizationId == organizationId && 
                    i.Email.ToLower() == request.Email.ToLower() &&
                    i.Status == InvitationStatus.Pending &&
                    i.ExpiresAt > DateTime.UtcNow);

            if (existingInvitation != null)
            {
                throw new BadRequestException("A pending invitation already exists for this email address.");
            }

            // Validate role
            if (!IsValidRole(request.Role))
            {
                throw new BadRequestException($"Invalid role: {request.Role}. Valid roles are: {Roles.Owner}, {Roles.OrganizationAdmin}, {Roles.Member}");
            }

            // Prevent inviting as Owner (only one owner per organization)
            if (request.Role == Roles.Owner)
            {
                throw new BadRequestException("Cannot invite a user as Owner. Only the organization creator can be the Owner.");
            }

            // Set default expiration (7 days from now)
            var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(7);

            // Create invitation
            var invitation = new Invitation
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Email = request.Email,
                UserId = existingUser?.Id,
                Role = request.Role,
                Status = InvitationStatus.Pending,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Invitations.Add(invitation);
            await _dbContext.SaveChangesAsync();

            var response = new InviteMemberResponse
            {
                InvitationId = invitation.Id,
                Email = invitation.Email,
                Role = invitation.Role,
                ExpiresAt = invitation.ExpiresAt,
                CreatedAt = invitation.CreatedAt
            };

            _logger.LogInformation("Invitation {InvitationId} created for email {Email} to organization {OrganizationId} by user {UserId}",
                invitation.Id, request.Email, organizationId, CurrentUserId);

            return CreatedAtAction(nameof(GetMember), new { organizationId, memberId = Guid.Empty }, response);
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
            _logger.LogError(ex, "Error inviting member to organization {OrganizationId}", organizationId);
            throw new BadRequestException("Failed to invite member. Please check your input and try again.");
        }
    }

    /// <summary>
    /// Gets a paginated list of members in an organization with optional filtering and sorting.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="request">Pagination and filtering parameters.</param>
    /// <returns>Paginated list of members.</returns>
    /// <response code="200">Returns the list of members.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="404">If the organization is not found.</response>
    /// <response code="403">If the user does not have access to this organization.</response>
    [HttpGet("{organizationId}")]
    [ProducesResponseType(typeof(GetMembersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMembers(Guid organizationId, [FromQuery] GetMembersRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verify organization exists
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Check authorization: Admins can access any organization, others must be members
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsMemberOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, organizationId);
            }

            // Build query
            var query = _dbContext.OrganizationMembers
                .Include(om => om.User)
                .Where(om => om.OrganizationId == organizationId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                query = query.Where(om => om.Role == request.Role);
            }

            if (!string.IsNullOrWhiteSpace(request.EmailFilter))
            {
                var emailFilter = request.EmailFilter.ToLower();
                query = query.Where(om => om.User.Email.ToLower().Contains(emailFilter));
            }

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(om =>
                    om.User.Email.ToLower().Contains(searchTerm) ||
                    om.User.Name.ToLower().Contains(searchTerm));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                var sortDirection = request.SortDirection?.ToLower() == "desc" ? "desc" : "asc";
                query = request.SortBy.ToLower() switch
                {
                    "email" => sortDirection == "desc"
                        ? query.OrderByDescending(om => om.User.Email)
                        : query.OrderBy(om => om.User.Email),
                    "name" => sortDirection == "desc"
                        ? query.OrderByDescending(om => om.User.Name)
                        : query.OrderBy(om => om.User.Name),
                    "role" => sortDirection == "desc"
                        ? query.OrderByDescending(om => om.Role)
                        : query.OrderBy(om => om.Role),
                    "joinedat" => sortDirection == "desc"
                        ? query.OrderByDescending(om => om.JoinedAt)
                        : query.OrderBy(om => om.JoinedAt),
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
            var members = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            // Get license assignment counts for each member
            var memberIds = members.Select(m => m.Id).ToList();
            var licenseAssignmentCounts = await _dbContext.LicenseAssignments
                .Where(la => memberIds.Contains(la.OrganizationMemberId) && la.UnassignedAt == null)
                .GroupBy(la => la.OrganizationMemberId)
                .Select(g => new { MemberId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.MemberId, x => x.Count);

            // Map to response DTOs
            var memberResponses = members.Select(member =>
            {
                licenseAssignmentCounts.TryGetValue(member.Id, out var licenseCount);
                return MapToMemberResponse(member, licenseCount);
            }).ToList();

            var response = new GetMembersResponse
            {
                Items = memberResponses,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

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
            _logger.LogError(ex, "Error retrieving members for organization {OrganizationId}", organizationId);
            throw new BadRequestException("Failed to retrieve members. Please check your query parameters and try again.");
        }
    }

    /// <summary>
    /// Gets a specific member by ID.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="memberId">Member ID (OrganizationMember ID).</param>
    /// <returns>Member information.</returns>
    /// <response code="200">Returns the member.</response>
    /// <response code="404">If the member or organization is not found.</response>
    /// <response code="403">If the user does not have access to this organization.</response>
    [HttpGet("{organizationId}/{memberId}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMember(Guid organizationId, Guid memberId)
    {
        try
        {
            // Verify organization exists
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Check authorization: Admins can access any organization, others must be members
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsMemberOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, organizationId);
            }

            var member = await _dbContext.OrganizationMembers
                .Include(om => om.User)
                .FirstOrDefaultAsync(om => om.Id == memberId && om.OrganizationId == organizationId);

            if (member == null)
            {
                throw new NotFoundException("Member", memberId);
            }

            // Get license assignment count
            var licenseAssignmentCount = await _dbContext.LicenseAssignments
                .CountAsync(la => la.OrganizationMemberId == member.Id && la.UnassignedAt == null);

            var response = MapToMemberResponse(member, licenseAssignmentCount);

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
            _logger.LogError(ex, "Error retrieving member {MemberId} from organization {OrganizationId}", memberId, organizationId);
            throw new NotFoundException("Member", memberId);
        }
    }

    /// <summary>
    /// Updates a member's role in an organization.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="memberId">Member ID (OrganizationMember ID).</param>
    /// <param name="request">Update request with new role.</param>
    /// <returns>Updated member information.</returns>
    /// <response code="200">Member role updated successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the member or organization is not found.</response>
    /// <response code="403">If the user does not have permission to update member roles.</response>
    [HttpPut("{organizationId}/{memberId}/role")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateMemberRole(Guid organizationId, Guid memberId, [FromBody] UpdateMemberRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verify organization exists
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Check authorization: Admins can update any organization, others must be Owner or OrganizationAdmin
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, organizationId);
            }

            var member = await _dbContext.OrganizationMembers
                .Include(om => om.User)
                .FirstOrDefaultAsync(om => om.Id == memberId && om.OrganizationId == organizationId);

            if (member == null)
            {
                throw new NotFoundException("Member", memberId);
            }

            // Validate role
            if (!IsValidRole(request.Role))
            {
                throw new BadRequestException($"Invalid role: {request.Role}. Valid roles are: {Roles.Owner}, {Roles.OrganizationAdmin}, {Roles.Member}");
            }

            // Prevent changing Owner role (only one owner per organization)
            if (member.Role == Roles.Owner && request.Role != Roles.Owner)
            {
                throw new BadRequestException("Cannot change the Owner's role. The Owner role cannot be removed.");
            }

            // Prevent assigning Owner role to non-owners
            if (request.Role == Roles.Owner && member.Role != Roles.Owner)
            {
                throw new BadRequestException("Cannot assign Owner role. Only the organization creator can be the Owner.");
            }

            // Prevent user from changing their own role
            if (member.UserId == CurrentUserIdGuid && !IsAdmin)
            {
                throw new BadRequestException("You cannot change your own role.");
            }

            // Update role
            member.Role = request.Role;
            member.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Get license assignment count
            var licenseAssignmentCount = await _dbContext.LicenseAssignments
                .CountAsync(la => la.OrganizationMemberId == member.Id && la.UnassignedAt == null);

            var response = MapToMemberResponse(member, licenseAssignmentCount);

            _logger.LogInformation("Member {MemberId} role updated to {Role} in organization {OrganizationId} by user {UserId}",
                memberId, request.Role, organizationId, CurrentUserId);

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
            _logger.LogError(ex, "Error updating member {MemberId} role in organization {OrganizationId}", memberId, organizationId);
            throw new BadRequestException("Failed to update member role. Please check your input and try again.");
        }
    }

    /// <summary>
    /// Removes a member from an organization.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="memberId">Member ID (OrganizationMember ID).</param>
    /// <returns>No content.</returns>
    /// <response code="204">Member removed successfully.</response>
    /// <response code="404">If the member or organization is not found.</response>
    /// <response code="403">If the user does not have permission to remove members.</response>
    /// <response code="400">If attempting to remove the Owner.</response>
    [HttpDelete("{organizationId}/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveMember(Guid organizationId, Guid memberId)
    {
        try
        {
            // Verify organization exists
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Check authorization: Admins can remove from any organization, others must be Owner or OrganizationAdmin
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, organizationId);
            }

            var member = await _dbContext.OrganizationMembers
                .Include(om => om.User)
                .FirstOrDefaultAsync(om => om.Id == memberId && om.OrganizationId == organizationId);

            if (member == null)
            {
                throw new NotFoundException("Member", memberId);
            }

            // Prevent removing the Owner
            if (member.Role == Roles.Owner)
            {
                throw new BadRequestException("Cannot remove the Owner from the organization.");
            }

            // Prevent user from removing themselves
            if (member.UserId == CurrentUserIdGuid && !IsAdmin)
            {
                throw new BadRequestException("You cannot remove yourself from the organization. Please use the leave organization endpoint.");
            }

            // Unassign all license assignments for this member
            var licenseAssignments = await _dbContext.LicenseAssignments
                .Where(la => la.OrganizationMemberId == member.Id && la.UnassignedAt == null)
                .ToListAsync();

            foreach (var assignment in licenseAssignments)
            {
                assignment.UnassignedAt = DateTime.UtcNow;
            }

            // Remove the membership
            _dbContext.OrganizationMembers.Remove(member);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Member {MemberId} removed from organization {OrganizationId} by user {UserId}",
                memberId, organizationId, CurrentUserId);

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
            _logger.LogError(ex, "Error removing member {MemberId} from organization {OrganizationId}", memberId, organizationId);
            throw new NotFoundException("Member", memberId);
        }
    }

    /// <summary>
    /// Maps an OrganizationMember entity to a MemberResponse DTO.
    /// </summary>
    private static MemberResponse MapToMemberResponse(OrganizationMember member, int assignedLicenseCount)
    {
        return new MemberResponse
        {
            Id = member.Id,
            UserId = member.UserId,
            Email = member.User.Email,
            Name = member.User.Name,
            Role = member.Role,
            JoinedAt = member.JoinedAt,
            AssignedLicenseCount = assignedLicenseCount,
            HasActiveLicense = assignedLicenseCount > 0
        };
    }

    /// <summary>
    /// Validates if a role is valid.
    /// </summary>
    private static bool IsValidRole(string role)
    {
        return role == Roles.Owner ||
               role == Roles.OrganizationAdmin ||
               role == Roles.Member;
    }
}
