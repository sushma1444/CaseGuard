using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Licenses.Requests;

/// <summary>
/// Request DTO for creating a new license.
/// </summary>
public class CreateLicenseRequest
{
    /// <summary>
    /// Organization ID that will own this license.
    /// </summary>
    [Required(ErrorMessage = "Organization ID is required.")]
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Name or description of the license.
    /// </summary>
    [Required(ErrorMessage = "License name is required.")]
    [StringLength(200, ErrorMessage = "License name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional start date for the license. If not provided, defaults to current time.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Optional expiration date. If not provided, defaults to 10 minutes from start date.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Whether auto-renewal should be enabled for this license.
    /// </summary>
    public bool AutoRenewalEnabled { get; set; } = false;
}
