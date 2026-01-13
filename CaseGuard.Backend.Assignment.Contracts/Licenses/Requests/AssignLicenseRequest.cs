using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Licenses.Requests;

/// <summary>
/// Request DTO for assigning a license to a user.
/// </summary>
public class AssignLicenseRequest
{
    /// <summary>
    /// License ID to assign.
    /// </summary>
    [Required(ErrorMessage = "License ID is required.")]
    public Guid LicenseId { get; set; }

    /// <summary>
    /// User ID to assign the license to.
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public Guid UserId { get; set; }
}
