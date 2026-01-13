using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Licenses.Requests;

/// <summary>
/// Request DTO for updating a license.
/// </summary>
public class UpdateLicenseRequest
{
    /// <summary>
    /// Updated name or description of the license.
    /// </summary>
    [StringLength(200, ErrorMessage = "License name cannot exceed 200 characters.")]
    public string? Name { get; set; }

    /// <summary>
    /// Updated expiration date.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Updated auto-renewal setting.
    /// </summary>
    public bool? AutoRenewalEnabled { get; set; }

    /// <summary>
    /// Updated active status.
    /// </summary>
    public bool? IsActive { get; set; }
}
