namespace Auth.Application.DTOs;

/// <summary>
/// DTO for refresh token request.
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
