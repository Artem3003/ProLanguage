using System.Text.Json;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Publishes notification messages to an Azure Service Bus queue.
/// </summary>
public class ServiceBusNotificationPublisher : INotificationPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusNotificationPublisher> _logger;

    public ServiceBusNotificationPublisher(IConfiguration configuration, ILogger<ServiceBusNotificationPublisher> logger)
    {
        _logger = logger;

        var connectionString = configuration.GetConnectionString("ServiceBus");
        var queueName = configuration["ServiceBus:NotificationQueueName"] ?? "notifications";

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("ServiceBus connection string is null or empty. Notification publishing will fail if invoked.");

            // We still initialize with a dummy to fail gracefully or you can throw here.
        }
        else
        {
            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);
        }
    }

    public async Task PublishNotificationAsync(NotificationMessageDto message)
    {
        if (_sender == null)
        {
            _logger.LogError("Cannot publish notification: ServiceBusSender is not initialized. Ensure connection string is configured.");
            return;
        }

        try
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage)
            {
                Subject = message.TargetMethod,
            };

            await _sender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("Successfully published {Method} notification to Service Bus.", message.TargetMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish notification to Service Bus.");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_sender != null)
        {
            await _sender.DisposeAsync();
        }

        if (_client != null)
        {
            await _client.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}