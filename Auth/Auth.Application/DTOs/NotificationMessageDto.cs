namespace Auth.Application.DTOs;

/// <summary>
/// Data transfer object for sending a notification.
/// </summary>
public class NotificationMessageDto
{
    /// <summary>
    /// Gets or sets the recipient email address.
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject of the notification.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body of the notification.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target method (e.g., "email", "sms", "push").
    /// </summary>
    public string TargetMethod { get; set; } = string.Empty;
}