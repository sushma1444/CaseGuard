namespace CaseGuard.Backend.Assignment.Services;

/// <summary>
/// Service for handling license expiration logic.
/// </summary>
public interface ILicenseExpirationService
{
    /// <summary>
    /// Checks and invalidates expired licenses.
    /// Sets IsActive to false for licenses that have passed their expiration date.
    /// </summary>
    /// <returns>Number of licenses that were invalidated.</returns>
    Task<int> InvalidateExpiredLicensesAsync();

    /// <summary>
    /// Checks if a license is expired.
    /// </summary>
    /// <param name="expirationDate">The expiration date to check.</param>
    /// <returns>True if the license is expired, false otherwise.</returns>
    bool IsLicenseExpired(DateTime expirationDate);

    /// <summary>
    /// Gets the number of days until expiration for a license.
    /// Returns negative value if already expired.
    /// </summary>
    /// <param name="expirationDate">The expiration date.</param>
    /// <returns>Number of days until expiration.</returns>
    double GetDaysUntilExpiration(DateTime expirationDate);
}
