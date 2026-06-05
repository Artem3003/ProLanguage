using System.Security.Claims;

namespace Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Resolves the authenticated user's unique identifier from the
    /// <see cref="ClaimTypes.NameIdentifier"/> claim issued by the Auth service.
    /// Returns <c>null</c> when the claim is missing or is not a valid <see cref="Guid"/>.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
