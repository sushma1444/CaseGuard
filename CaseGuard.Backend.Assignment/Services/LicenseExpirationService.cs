using CaseGuard.Backend.Assignment.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CaseGuard.Backend.Assignment.Services;

/// <summary>
/// Service for handling license expiration logic.
/// </summary>
public class LicenseExpirationService : ILicenseExpirationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LicenseExpirationService> _logger;

    public LicenseExpirationService(
        ApplicationDbContext dbContext,
        ILogger<LicenseExpirationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Checks and invalidates expired licenses.
    /// Sets IsActive to false for licenses that have passed their expiration date.
    /// </summary>
    /// <returns>Number of licenses that were invalidated.</returns>
    public async Task<int> InvalidateExpiredLicensesAsync()
    {
        var now = DateTime.UtcNow;
        
        // Find all active licenses that have expired
        var expiredLicenses = await _dbContext.Licenses
            .Where(l => l.IsActive && 
                       l.ExpirationDate <= now && 
                       l.CancelledAt == null)
            .ToListAsync();

        if (expiredLicenses.Count == 0)
        {
            _logger.LogDebug("No expired licenses found to invalidate.");
            return 0;
        }

        // Invalidate expired licenses
        foreach (var license in expiredLicenses)
        {
            license.IsActive = false;
            license.UpdatedAt = now;
            
            _logger.LogInformation(
                "License {LicenseId} (Organization: {OrganizationId}, Name: {LicenseName}) expired and has been invalidated. Expiration date: {ExpirationDate}",
                license.Id, license.OrganizationId, license.Name, license.ExpirationDate);
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Invalidated {Count} expired license(s).", expiredLicenses.Count);
        
        return expiredLicenses.Count;
    }

    /// <summary>
    /// Checks if a license is expired.
    /// </summary>
    /// <param name="expirationDate">The expiration date to check.</param>
    /// <returns>True if the license is expired, false otherwise.</returns>
    public bool IsLicenseExpired(DateTime expirationDate)
    {
        return expirationDate <= DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the number of days until expiration for a license.
    /// Returns negative value if already expired.
    /// </summary>
    /// <param name="expirationDate">The expiration date.</param>
    /// <returns>Number of days until expiration.</returns>
    public double GetDaysUntilExpiration(DateTime expirationDate)
    {
        var now = DateTime.UtcNow;
        var timeSpan = expirationDate - now;
        return timeSpan.TotalDays;
    }
}
