using CaseGuard.Backend.Assignment.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CaseGuard.Backend.Assignment.Services;

/// <summary>
/// Service for handling license auto-renewal.
/// </summary>
public class LicenseRenewalService : ILicenseRenewalService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LicenseRenewalService> _logger;

    public LicenseRenewalService(
        ApplicationDbContext dbContext,
        ILogger<LicenseRenewalService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Renews licenses that are eligible for auto-renewal.
    /// Licenses are renewed if they have AutoRenewalEnabled = true and are within the renewal window.
    /// </summary>
    /// <param name="renewalWindowDays">Number of days before expiration to trigger renewal. Default is 7 days.</param>
    /// <returns>Number of licenses that were renewed.</returns>
    public async Task<int> RenewEligibleLicensesAsync(int renewalWindowDays = 7)
    {
        var now = DateTime.UtcNow;
        var renewalThreshold = now.AddDays(renewalWindowDays);

        // Find licenses eligible for renewal:
        // - AutoRenewalEnabled = true
        // - IsActive = true
        // - Not cancelled
        // - ExpirationDate is within the renewal window (between now and renewalThreshold)
        var eligibleLicenses = await _dbContext.Licenses
            .Where(l => l.AutoRenewalEnabled &&
                       l.IsActive &&
                       l.CancelledAt == null &&
                       l.ExpirationDate > now &&
                       l.ExpirationDate <= renewalThreshold)
            .ToListAsync();

        if (eligibleLicenses.Count == 0)
        {
            _logger.LogDebug("No licenses found eligible for auto-renewal.");
            return 0;
        }

        var renewedCount = 0;

        foreach (var license in eligibleLicenses)
        {
            try
            {
                // Calculate new expiration date: extend by the same duration as the original license
                var originalDuration = license.ExpirationDate - license.StartDate;
                var newExpirationDate = license.ExpirationDate.Add(originalDuration);

                // Update license
                license.ExpirationDate = newExpirationDate;
                license.UpdatedAt = now;

                renewedCount++;

                _logger.LogInformation(
                    "License {LicenseId} (Organization: {OrganizationId}, Name: {LicenseName}) auto-renewed. " +
                    "Old expiration: {OldExpiration}, New expiration: {NewExpiration}",
                    license.Id, license.OrganizationId, license.Name,
                    license.ExpirationDate.Subtract(originalDuration), newExpirationDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error renewing license {LicenseId} (Organization: {OrganizationId})",
                    license.Id, license.OrganizationId);
                // Continue with other licenses even if one fails
            }
        }

        if (renewedCount > 0)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Auto-renewed {Count} license(s).", renewedCount);
        }

        return renewedCount;
    }
}
