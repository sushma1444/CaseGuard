using CaseGuard.Backend.Assignment.Contracts.Licenses.Requests;
using CaseGuard.Backend.Assignment.Contracts.Licenses.Responses;
using CaseGuard.Backend.Assignment.Data;
using CaseGuard.Backend.Assignment.Entities;
using CaseGuard.Backend.Assignment.Exceptions;
using CaseGuard.Backend.Assignment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaseGuard.Backend.Assignment.Controllers;

/// <summary>
/// Controller for license management operations (Admin only).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class LicenseController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LicenseController> _logger;
    private readonly ILicenseExpirationService _expirationService;

    public LicenseController(
        ApplicationDbContext dbContext,
        ILogger<LicenseController> logger,
        ILicenseExpirationService expirationService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _expirationService = expirationService;
    }

    /// <summary>
    /// Creates a new license for an organization.
    /// </summary>
    /// <param name="request">License creation request.</param>
    /// <returns>Created license information.</returns>
    /// <response code="201">License created successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the organization is not found.</response>
    /// <response code="403">If the user is not an admin.</response>
    [HttpPost]
    [ProducesResponseType(typeof(LicenseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verify organization exists
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == request.OrganizationId);

            if (organization == null)
            {
                throw new NotFoundException("Organization", request.OrganizationId);
            }

            // Set default dates if not provided
            var startDate = request.StartDate ?? DateTime.UtcNow;
            var expirationDate = request.ExpirationDate ?? startDate.AddMinutes(10); // Default 10 minutes for testing

            // Validate expiration date is after start date
            if (expirationDate <= startDate)
            {
                throw new BadRequestException("Expiration date must be after start date.");
            }

            // Create license
            var license = new License
            {
                Id = Guid.NewGuid(),
                OrganizationId = request.OrganizationId,
                Name = request.Name,
                StartDate = startDate,
                ExpirationDate = expirationDate,
                AutoRenewalEnabled = request.AutoRenewalEnabled,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Licenses.Add(license);
            await _dbContext.SaveChangesAsync();

            // Load organization for response
            await _dbContext.Entry(license)
                .Reference(l => l.Organization)
                .LoadAsync();

            // Get assigned user count
            var assignedUserCount = await _dbContext.LicenseAssignments
                .CountAsync(la => la.LicenseId == license.Id && la.UnassignedAt == null);

            var response = MapToLicenseResponse(license, assignedUserCount);

            _logger.LogInformation("License {LicenseId} created for organization {OrganizationId} by admin {AdminId}",
                license.Id, request.OrganizationId, CurrentUserId);

            return CreatedAtAction(nameof(GetLicense), new { id = license.Id }, response);
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
            _logger.LogError(ex, "Error creating license for organization {OrganizationId}", request.OrganizationId);
            throw new BadRequestException("Failed to create license. Please check your input and try again.");
        }
    }

    /// <summary>
    /// Gets a paginated list of licenses with optional filtering and sorting.
    /// </summary>
    /// <param name="request">Pagination and filtering parameters.</param>
    /// <returns>Paginated list of licenses.</returns>
    /// <response code="200">Returns the list of licenses.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="403">If the user is not an admin.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetLicensesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLicenses([FromQuery] GetLicensesRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Check and invalidate expired licenses before querying
            await _expirationService.InvalidateExpiredLicensesAsync();

            // Build query
            var query = _dbContext.Licenses
                .Include(l => l.Organization)
                .AsQueryable();

            // Apply filters
            if (request.OrganizationId.HasValue)
            {
                query = query.Where(l => l.OrganizationId == request.OrganizationId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(l => l.IsActive == request.IsActive.Value);
            }

            if (request.AutoRenewalEnabled.HasValue)
            {
                query = query.Where(l => l.AutoRenewalEnabled == request.AutoRenewalEnabled.Value);
            }

            // Filter by expiration status
            if (!string.IsNullOrWhiteSpace(request.ExpirationStatus))
            {
                var now = DateTime.UtcNow;
                switch (request.ExpirationStatus.ToLower())
                {
                    case "expired":
                        query = query.Where(l => l.ExpirationDate < now);
                        break;
                    case "active":
                        query = query.Where(l => l.ExpirationDate >= now);
                        break;
                    case "all":
                        // No filter
                        break;
                    default:
                        throw new BadRequestException($"Invalid expiration status: {request.ExpirationStatus}. Valid values are: expired, active, all");
                }
            }

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(l =>
                    l.Name.ToLower().Contains(searchTerm) ||
                    l.Organization.Name.ToLower().Contains(searchTerm));
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
                        ? query.OrderByDescending(l => l.Name)
                        : query.OrderBy(l => l.Name),
                    "startdate" => sortDirection == "desc"
                        ? query.OrderByDescending(l => l.StartDate)
                        : query.OrderBy(l => l.StartDate),
                    "expirationdate" => sortDirection == "desc"
                        ? query.OrderByDescending(l => l.ExpirationDate)
                        : query.OrderBy(l => l.ExpirationDate),
                    "createdat" => sortDirection == "desc"
                        ? query.OrderByDescending(l => l.CreatedAt)
                        : query.OrderBy(l => l.CreatedAt),
                    "organizationname" => sortDirection == "desc"
                        ? query.OrderByDescending(l => l.Organization.Name)
                        : query.OrderBy(l => l.Organization.Name),
                    _ => query.OrderByDescending(l => l.CreatedAt) // Default sort
                };
            }
            else
            {
                // Default sort by creation date (newest first)
                query = query.OrderByDescending(l => l.CreatedAt);
            }

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            var licenses = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            // Get assigned user counts for each license
            var licenseIds = licenses.Select(l => l.Id).ToList();
            var assignedCounts = await _dbContext.LicenseAssignments
                .Where(la => licenseIds.Contains(la.LicenseId) && la.UnassignedAt == null)
                .GroupBy(la => la.LicenseId)
                .Select(g => new { LicenseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.LicenseId, x => x.Count);

            // Map to response DTOs
            var licenseResponses = licenses.Select(license =>
            {
                assignedCounts.TryGetValue(license.Id, out var count);
                return MapToLicenseResponse(license, count);
            }).ToList();

            var response = new GetLicensesResponse
            {
                Items = licenseResponses,
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
            _logger.LogError(ex, "Error retrieving licenses");
            throw new BadRequestException("Failed to retrieve licenses. Please check your query parameters and try again.");
        }
    }

    /// <summary>
    /// Gets a specific license by ID.
    /// </summary>
    /// <param name="id">License ID.</param>
    /// <returns>License information.</returns>
    /// <response code="200">Returns the license.</response>
    /// <response code="404">If the license is not found.</response>
    /// <response code="403">If the user is not an admin.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LicenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLicense(Guid id)
    {
        try
        {
            // Check and invalidate expired licenses before querying
            await _expirationService.InvalidateExpiredLicensesAsync();

            var license = await _dbContext.Licenses
                .Include(l => l.Organization)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (license == null)
            {
                throw new NotFoundException("License", id);
            }

            // Get assigned user count
            var assignedUserCount = await _dbContext.LicenseAssignments
                .CountAsync(la => la.LicenseId == license.Id && la.UnassignedAt == null);

            var response = MapToLicenseResponse(license, assignedUserCount);

            return Ok(response);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving license {LicenseId}", id);
            throw new NotFoundException("License", id);
        }
    }

    /// <summary>
    /// Updates a license's properties.
    /// </summary>
    /// <param name="id">License ID.</param>
    /// <param name="request">Update request with properties to modify.</param>
    /// <returns>Updated license information.</returns>
    /// <response code="200">License updated successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the license is not found.</response>
    /// <response code="403">If the user is not an admin.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(LicenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateLicense(Guid id, [FromBody] UpdateLicenseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var license = await _dbContext.Licenses
                .Include(l => l.Organization)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (license == null)
            {
                throw new NotFoundException("License", id);
            }

            // Update properties if provided
            if (request.Name != null)
            {
                license.Name = request.Name;
            }

            if (request.ExpirationDate.HasValue)
            {
                // Validate expiration date is after start date
                if (request.ExpirationDate.Value <= license.StartDate)
                {
                    throw new BadRequestException("Expiration date must be after start date.");
                }
                license.ExpirationDate = request.ExpirationDate.Value;
            }

            if (request.AutoRenewalEnabled.HasValue)
            {
                license.AutoRenewalEnabled = request.AutoRenewalEnabled.Value;
            }

            if (request.IsActive.HasValue)
            {
                license.IsActive = request.IsActive.Value;
            }

            license.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Get assigned user count
            var assignedUserCount = await _dbContext.LicenseAssignments
                .CountAsync(la => la.LicenseId == license.Id && la.UnassignedAt == null);

            var response = MapToLicenseResponse(license, assignedUserCount);

            _logger.LogInformation("License {LicenseId} updated by admin {AdminId}", id, CurrentUserId);

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
            _logger.LogError(ex, "Error updating license {LicenseId}", id);
            throw new BadRequestException("Failed to update license. Please check your input and try again.");
        }
    }

    /// <summary>
    /// Checks and invalidates expired licenses.
    /// This endpoint manually triggers the expiration check process.
    /// </summary>
    /// <returns>Number of licenses that were invalidated.</returns>
    /// <response code="200">Expiration check completed successfully.</response>
    /// <response code="403">If the user is not an admin.</response>
    [HttpPost("check-expiration")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckAndInvalidateExpiredLicenses()
    {
        try
        {
            var invalidatedCount = await _expirationService.InvalidateExpiredLicensesAsync();

            _logger.LogInformation("Expiration check completed by admin {AdminId}. Invalidated {Count} license(s).",
                CurrentUserId, invalidatedCount);

            return Ok(new { InvalidatedCount = invalidatedCount, Message = $"Invalidated {invalidatedCount} expired license(s)." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking license expiration");
            throw new BadRequestException("Failed to check license expiration. Please try again.");
        }
    }

    /// <summary>
    /// Cancels (revokes) a license.
    /// </summary>
    /// <param name="id">License ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">License cancelled successfully.</response>
    /// <response code="404">If the license is not found.</response>
    /// <response code="403">If the user is not an admin.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelLicense(Guid id)
    {
        try
        {
            var license = await _dbContext.Licenses
                .FirstOrDefaultAsync(l => l.Id == id);

            if (license == null)
            {
                throw new NotFoundException("License", id);
            }

            // Cancel the license
            license.IsActive = false;
            license.CancelledAt = DateTime.UtcNow;
            license.UpdatedAt = DateTime.UtcNow;

            // Also deactivate all license assignments
            var assignments = await _dbContext.LicenseAssignments
                .Where(la => la.LicenseId == id && la.UnassignedAt == null)
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                assignment.UnassignedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("License {LicenseId} cancelled by admin {AdminId}", id, CurrentUserId);

            return NoContent();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling license {LicenseId}", id);
            throw new NotFoundException("License", id);
        }
    }

    /// <summary>
    /// Maps a License entity to a LicenseResponse DTO.
    /// </summary>
    private static LicenseResponse MapToLicenseResponse(License license, int assignedUserCount)
    {
        return new LicenseResponse
        {
            Id = license.Id,
            OrganizationId = license.OrganizationId,
            OrganizationName = license.Organization?.Name ?? string.Empty,
            Name = license.Name,
            StartDate = license.StartDate,
            ExpirationDate = license.ExpirationDate,
            AutoRenewalEnabled = license.AutoRenewalEnabled,
            IsActive = license.IsActive,
            IsValid = license.IsValid,
            CreatedAt = license.CreatedAt,
            UpdatedAt = license.UpdatedAt,
            CancelledAt = license.CancelledAt,
            AssignedUserCount = assignedUserCount
        };
    }
}
