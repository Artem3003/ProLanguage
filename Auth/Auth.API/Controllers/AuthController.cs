using System.Security.Claims;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthController"/> class.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>The authentication response with tokens.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Invalid request or user already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var ipAddress = GetIpAddress();
        var result = await _authService.RegisterAsync(request, ipAddress);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns tokens.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>The authentication response with tokens.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Invalid credentials.</response>
    /// <response code="401">Account locked or deactivated.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var ipAddress = GetIpAddress();
        var result = await _authService.LoginAsync(request, ipAddress);

        if (!result.IsSuccess)
        {
            return result.ErrorMessage?.Contains("locked") == true || result.ErrorMessage?.Contains("deactivated") == true
                ? (ActionResult<AuthResponseDto>)Unauthorized(result)
                : (ActionResult<AuthResponseDto>)BadRequest(result);
        }

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <returns>The authentication response with new tokens.</returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="400">Invalid or expired refresh token.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        // Try to get refresh token from cookie if not provided in body
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            request.RefreshToken = Request.Cookies["refreshToken"] ?? string.Empty;
        }

        var ipAddress = GetIpAddress();
        var result = await _authService.RefreshTokenAsync(request, ipAddress);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke (optional, will use cookie if not provided).</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="200">Token revoked successfully.</response>
    /// <response code="400">Invalid token.</response>
    [HttpPost("revoke-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeToken([FromBody] string? refreshToken)
    {
        var token = refreshToken ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Refresh token is required." });
        }

        var ipAddress = GetIpAddress();
        var result = await _authService.RevokeTokenAsync(token, ipAddress);

        if (!result)
        {
            return BadRequest(new { message = "Token not found or already revoked." });
        }

        // Clear the cookie
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Token revoked successfully." });
    }

    /// <summary>
    /// Logs out the current user by revoking all their refresh tokens.
    /// </summary>
    /// <returns>Success response.</returns>
    /// <response code="200">Logged out successfully.</response>
    /// <response code="401">User not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var ipAddress = GetIpAddress();
        await _authService.LogoutAsync(userId, ipAddress);

        // Clear the cookie
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Authenticates a user using an external provider (Google, Apple, etc.).
    /// </summary>
    /// <param name="request">The external login request containing provider and ID token.</param>
    /// <returns>The authentication response with tokens.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Invalid request or token.</response>
    /// <response code="401">Account deactivated.</response>
    [HttpPost("external-login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> ExternalLogin([FromBody] ExternalLoginDto request)
    {
        var ipAddress = GetIpAddress();
        var result = await _authService.ExternalLoginAsync(request, ipAddress);

        if (!result.IsSuccess)
        {
            return result.ErrorMessage?.Contains("deactivated") == true
                ? (ActionResult<AuthResponseDto>)Unauthorized(result)
                : (ActionResult<AuthResponseDto>)BadRequest(result);
        }

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    private string? GetIpAddress()
    {
        return Request.Headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value)
            ? value.ToString()
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
