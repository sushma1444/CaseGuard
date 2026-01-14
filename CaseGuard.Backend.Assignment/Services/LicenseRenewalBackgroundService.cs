using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CaseGuard.Backend.Assignment.Services;

/// <summary>
/// Background service that periodically checks and renews licenses with auto-renewal enabled.
/// </summary>
public class LicenseRenewalBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LicenseRenewalBackgroundService> _logger;
    private readonly TimeSpan _checkInterval;

    public LicenseRenewalBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<LicenseRenewalBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        // Check every hour for licenses that need renewal
        _checkInterval = TimeSpan.FromHours(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("License Renewal Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformRenewalCheckAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in license renewal background service");
            }

            // Wait for the next check interval
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("License Renewal Background Service stopped.");
    }

    private async Task PerformRenewalCheckAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var renewalService = scope.ServiceProvider.GetRequiredService<ILicenseRenewalService>();
        var expirationService = scope.ServiceProvider.GetRequiredService<ILicenseExpirationService>();

        try
        {
            // First, invalidate any expired licenses
            var invalidatedCount = await expirationService.InvalidateExpiredLicensesAsync();
            if (invalidatedCount > 0)
            {
                _logger.LogInformation("Invalidated {Count} expired license(s) during renewal check.", invalidatedCount);
            }

            // Then, renew eligible licenses
            var renewedCount = await renewalService.RenewEligibleLicensesAsync(renewalWindowDays: 7);
            if (renewedCount > 0)
            {
                _logger.LogInformation("Auto-renewed {Count} license(s) during renewal check.", renewedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing license renewal check");
        }
    }
}
