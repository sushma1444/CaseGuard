using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Organizations.Requests;

/// <summary>
/// Request DTO for updating an organization.
/// </summary>
public class UpdateOrganizationRequest
{
    /// <summary>
    /// Updated name of the organization.
    /// </summary>
    [StringLength(200, ErrorMessage = "Organization name cannot exceed 200 characters.")]
    public string? Name { get; set; }

    /// <summary>
    /// Updated description of the organization.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }
}
