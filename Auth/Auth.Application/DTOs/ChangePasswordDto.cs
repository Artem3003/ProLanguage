namespace Auth.Application.DTOs;

/// <summary>
/// DTO for changing password request.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Gets or sets the current password.
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password confirmation.
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
