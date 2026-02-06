namespace Auth.Application.DTOs;

/// <summary>
/// DTO for external authentication login request.
/// </summary>
public class ExternalLoginDto
{
    /// <summary>
    /// Gets or sets the authentication provider (e.g., "Google", "Apple").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID token received from the external provider.
    /// </summary>
    public string IdToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name (optional, provided by Apple on first authorization).
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the user's last name (optional, provided by Apple on first authorization).
    /// </summary>
    public string? LastName { get; set; }
}
