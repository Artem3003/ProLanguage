using System.Security.Claims;
using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

/// <summary>
/// Interface for JWT token service.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The JWT token and its expiration time.</returns>
    (string Token, DateTime Expiration) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>The refresh token entity.</returns>
    RefreshToken GenerateRefreshToken(string? ipAddress);

    /// <summary>
    /// Validates a JWT access token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The claims principal if valid, null otherwise.</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Gets the user ID from a JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>The user ID.</returns>
    Guid? GetUserIdFromToken(string token);
}
