using CaseGuard.Backend.Assignment.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseGuard.Backend.Assignment.Controllers;

/// <summary>
/// Base controller with common functionality for all controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user's ID from JWT claims.
    /// </summary>
    protected string CurrentUserId => ClaimsHelper.GetUserId(User);

    /// <summary>
    /// Gets the current user's ID as a Guid from JWT claims.
    /// </summary>
    protected Guid CurrentUserIdGuid => ClaimsHelper.GetUserIdAsGuid(User);

    /// <summary>
    /// Gets the current user's email from JWT claims.
    /// </summary>
    protected string CurrentUserEmail => ClaimsHelper.GetEmail(User);

    /// <summary>
    /// Gets the current user's role from JWT claims.
    /// </summary>
    protected string CurrentUserRole => ClaimsHelper.GetRole(User);

    /// <summary>
    /// Checks if the current user is an admin.
    /// </summary>
    protected bool IsAdmin => ClaimsHelper.IsAdmin(User);

    /// <summary>
    /// Checks if the current user is an owner or organization admin.
    /// </summary>
    protected bool IsOwnerOrOrganizationAdmin => ClaimsHelper.IsOwnerOrOrganizationAdmin(User);

    /// <summary>
    /// Gets the organization ID from the current user's claims (if present).
    /// </summary>
    protected Guid? CurrentOrganizationId => ClaimsHelper.GetOrganizationId(User);
}
