namespace CaseGuard.Backend.Assignment.Services;

/// <summary>
/// Service for handling license auto-renewal.
/// </summary>
public interface ILicenseRenewalService
{
    /// <summary>
    /// Renews licenses that are eligible for auto-renewal.
    /// Licenses are renewed if they have AutoRenewalEnabled = true and are within the renewal window.
    /// </summary>
    /// <param name="renewalWindowDays">Number of days before expiration to trigger renewal. Default is 7 days.</param>
    /// <returns>Number of licenses that were renewed.</returns>
    Task<int> RenewEligibleLicensesAsync(int renewalWindowDays = 7);
}
