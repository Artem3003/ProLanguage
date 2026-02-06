namespace Auth.Application.DTOs;

/// <summary>
/// Internal DTO for holding user information from external providers.
/// </summary>
public class ExternalUserInfo
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier from the external provider.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;
}
