using CaseGuard.Backend.Assignment.Contracts.Invitations.Requests;
using CaseGuard.Backend.Assignment.Contracts.Invitations.Responses;
using CaseGuard.Backend.Assignment.Data;
using CaseGuard.Backend.Assignment.Entities;
using CaseGuard.Backend.Assignment.Exceptions;
using CaseGuard.Backend.Assignment.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaseGuard.Backend.Assignment.Controllers;

/// <summary>
/// Controller for invitation management operations.
/// Only Owners and OrganizationAdmins can view and manage invitations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvitationController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<InvitationController> _logger;

    public InvitationController(
        ApplicationDbContext dbContext,
        ILogger<InvitationController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of invitations for an organization with optional filtering and sorting.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="request">Pagination and filtering parameters.</param>
    /// <returns>Paginated list of invitations.</returns>
    /// <response code="200">Returns the list of invitations.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="404">If the organization is not found.</response>
    /// <response code="403">If the user does not have access to this organization.</response>
    [HttpGet("{organizationId}")]
    [ProducesResponseType(typeof(GetInvitationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInvitations(Guid organizationId, [FromQuery] GetInvitationsRequest request)
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

            // Check authorization: Admins can access any organization, others must be Owner or OrganizationAdmin
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, organizationId);
            }

            // Build query
            var query = _dbContext.Invitations
                .Include(i => i.Organization)
                .Where(i => i.OrganizationId == organizationId)
                .AsQueryable();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var statusLower = request.Status.ToLower();
                var status = statusLower switch
                {
                    "pending" => InvitationStatus.Pending,
                    "accepted" => InvitationStatus.Accepted,
                    "cancelled" => InvitationStatus.Cancelled,
                    "expired" => InvitationStatus.Expired,
                    _ => throw new BadRequestException($"Invalid status: {request.Status}. Valid values are: Pending, Accepted, Cancelled, Expired")
                };
                query = query.Where(i => i.Status == status);
            }

            // Apply email filter
            if (!string.IsNullOrWhiteSpace(request.EmailFilter))
            {
                var emailFilter = request.EmailFilter.ToLower();
                query = query.Where(i => i.Email.ToLower().Contains(emailFilter));
            }

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(i =>
                    i.Email.ToLower().Contains(searchTerm) ||
                    i.Organization.Name.ToLower().Contains(searchTerm));
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
                        ? query.OrderByDescending(i => i.Email)
                        : query.OrderBy(i => i.Email),
                    "status" => sortDirection == "desc"
                        ? query.OrderByDescending(i => i.Status)
                        : query.OrderBy(i => i.Status),
                    "createdat" => sortDirection == "desc"
                        ? query.OrderByDescending(i => i.CreatedAt)
                        : query.OrderBy(i => i.CreatedAt),
                    "expiresat" => sortDirection == "desc"
                        ? query.OrderByDescending(i => i.ExpiresAt)
                        : query.OrderBy(i => i.ExpiresAt),
                    _ => query.OrderByDescending(i => i.CreatedAt) // Default sort
                };
            }
            else
            {
                // Default sort by creation date (newest first)
                query = query.OrderByDescending(i => i.CreatedAt);
            }

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            var invitations = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to response DTOs
            var invitationResponses = invitations.Select(invitation =>
                MapToInvitationResponse(invitation)
            ).ToList();

            var response = new GetInvitationsResponse
            {
                Items = invitationResponses,
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
            _logger.LogError(ex, "Error retrieving invitations for organization {OrganizationId}", organizationId);
            throw new BadRequestException("Failed to retrieve invitations. Please check your query parameters and try again.");
        }
    }

    /// <summary>
    /// Gets a specific invitation by ID.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="invitationId">Invitation ID.</param>
    /// <returns>Invitation information.</returns>
    /// <response code="200">Returns the invitation.</response>
    /// <response code="404">If the invitation or organization is not found.</response>
    /// <response code="403">If the user does not have access to this organization.</response>
    [HttpGet("{organizationId}/{invitationId}")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInvitation(Guid organizationId, Guid invitationId)
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

            // Check authorization: Admins can access any organization, others must be Owner or OrganizationAdmin
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, organizationId);
            }

            var invitation = await _dbContext.Invitations
                .Include(i => i.Organization)
                .FirstOrDefaultAsync(i => i.Id == invitationId && i.OrganizationId == organizationId);

            if (invitation == null)
            {
                throw new NotFoundException("Invitation", invitationId);
            }

            var response = MapToInvitationResponse(invitation);

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
            _logger.LogError(ex, "Error retrieving invitation {InvitationId} from organization {OrganizationId}", invitationId, organizationId);
            throw new NotFoundException("Invitation", invitationId);
        }
    }

    /// <summary>
    /// Cancels a pending invitation.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="invitationId">Invitation ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Invitation cancelled successfully.</response>
    /// <response code="404">If the invitation or organization is not found.</response>
    /// <response code="403">If the user does not have permission to cancel invitations.</response>
    /// <response code="400">If the invitation is not in a cancellable state.</response>
    [HttpDelete("{organizationId}/{invitationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelInvitation(Guid organizationId, Guid invitationId)
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

            // Check authorization: Admins can cancel from any organization, others must be Owner or OrganizationAdmin
            if (!IsAdmin)
            {
                await AuthorizationHelper.EnsureUserIsOwnerOrAdminOfOrganizationAsync(
                    _dbContext, CurrentUserIdGuid, organizationId);
            }

            var invitation = await _dbContext.Invitations
                .FirstOrDefaultAsync(i => i.Id == invitationId && i.OrganizationId == organizationId);

            if (invitation == null)
            {
                throw new NotFoundException("Invitation", invitationId);
            }

            // Check if invitation can be cancelled
            if (invitation.Status != InvitationStatus.Pending)
            {
                throw new BadRequestException($"Cannot cancel invitation. Current status is: {invitation.Status}. Only pending invitations can be cancelled.");
            }

            // Check if invitation is already expired
            if (invitation.ExpiresAt <= DateTime.UtcNow)
            {
                // Mark as expired instead of cancelled
                invitation.Status = InvitationStatus.Expired;
                invitation.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Cancel the invitation
                invitation.Status = InvitationStatus.Cancelled;
                invitation.CancelledAt = DateTime.UtcNow;
                invitation.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Invitation {InvitationId} cancelled for organization {OrganizationId} by user {UserId}",
                invitationId, organizationId, CurrentUserId);

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
            _logger.LogError(ex, "Error cancelling invitation {InvitationId} from organization {OrganizationId}", invitationId, organizationId);
            throw new NotFoundException("Invitation", invitationId);
        }
    }

    /// <summary>
    /// Maps an Invitation entity to an InvitationResponse DTO.
    /// </summary>
    private static InvitationResponse MapToInvitationResponse(Invitation invitation)
    {
        return new InvitationResponse
        {
            Id = invitation.Id,
            OrganizationId = invitation.OrganizationId,
            OrganizationName = invitation.Organization?.Name ?? string.Empty,
            Email = invitation.Email,
            UserId = invitation.UserId,
            Role = invitation.Role,
            Status = invitation.Status.ToString(),
            ExpiresAt = invitation.ExpiresAt,
            IsValid = invitation.IsValid,
            CreatedAt = invitation.CreatedAt,
            UpdatedAt = invitation.UpdatedAt,
            AcceptedAt = invitation.AcceptedAt,
            CancelledAt = invitation.CancelledAt
        };
    }
}
