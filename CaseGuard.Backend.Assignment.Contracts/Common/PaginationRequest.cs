using System.ComponentModel.DataAnnotations;

namespace CaseGuard.Backend.Assignment.Contracts.Common;

/// <summary>
/// Base class for paginated requests.
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number (1-based).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Field to sort by.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc or desc).
    /// </summary>
    public string? SortDirection { get; set; } = "asc";

    /// <summary>
    /// Search term for filtering.
    /// </summary>
    public string? SearchTerm { get; set; }
}
