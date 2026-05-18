using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UrbanCommunity.Api.Controllers;

/// <summary>
/// Base controller for all API controllers.
/// Provides common helper methods
/// </summary>
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Extracts the authenticated user's ID from the JWT token.
    /// Used in [Authorize] endpoints; throws an exception if the user is not found.
    /// </summary>
    protected Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");

        return Guid.Parse(claim);
    }

    /// <summary>
    /// Attempts to extract the user's ID from the JWT token.
    /// Used in [AllowAnonymous] endpoints or where user context is optional.
    /// Returns null if the user is not authenticated.
    /// </summary>
    protected Guid? TryGetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
