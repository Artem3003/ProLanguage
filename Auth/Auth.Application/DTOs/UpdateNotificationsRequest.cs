namespace Auth.Application.DTOs;

/// <summary>
/// Data transfer object for updating user notifications.
/// </summary>
public class UpdateNotificationsRequest
{
    /// <summary>
    /// Gets or sets the preferred notification methods.
    /// </summary>
    public List<string> Notifications { get; set; } = [];
}
