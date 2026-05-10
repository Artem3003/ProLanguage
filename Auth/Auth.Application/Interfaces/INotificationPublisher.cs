using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;

/// <summary>
/// Interface for publishing notifications to the underlying infrastructure.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Publishes a notification message to be processed asynchronously.
    /// </summary>
    /// <param name="message">The notification message to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishNotificationAsync(NotificationMessageDto message);
}