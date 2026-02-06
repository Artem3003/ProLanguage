using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;

/// <summary>
/// Interface for authentication service.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>The authentication response.</returns>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string? ipAddress);

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>The authentication response.</returns>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress);

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>The authentication response.</returns>
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>True if the token was revoked successfully.</returns>
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress);

    /// <summary>
    /// Logs out a user by revoking all their refresh tokens.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>True if logout was successful.</returns>
    Task<bool> LogoutAsync(Guid userId, string? ipAddress);

    /// <summary>
    /// Authenticates a user using an external provider (Google, Apple, etc.).
    /// </summary>
    /// <param name="request">The external login request containing provider and ID token.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>The authentication response.</returns>
    Task<AuthResponseDto> ExternalLoginAsync(ExternalLoginDto request, string? ipAddress);
}
