using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Organizations.Requests;

/// <summary>
/// Request DTO for creating a new organization.
/// </summary>
public class CreateOrganizationRequest
{
    /// <summary>
    /// Name of the organization.
    /// </summary>
    [Required(ErrorMessage = "Organization name is required.")]
    [StringLength(200, ErrorMessage = "Organization name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the organization.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }
}
