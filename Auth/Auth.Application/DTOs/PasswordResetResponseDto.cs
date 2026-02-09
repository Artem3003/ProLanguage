namespace Auth.Application.DTOs;

/// <summary>
/// DTO for password reset response.
/// </summary>
public class PasswordResetResponseDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the error message if operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the reset token (only returned in development for testing).
    /// </summary>
    public string? ResetToken { get; set; }
}
