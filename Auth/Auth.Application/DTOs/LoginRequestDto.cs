namespace Auth.Application.DTOs;

/// <summary>
/// DTO for login request.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to remember the user.
    /// </summary>
    public bool RememberMe { get; set; }
}
