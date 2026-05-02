using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Constants;
using Application.DTOs.AiChat;
using Application.Interfaces;
using Domain.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class AiChatService(
    IOptions<AzureOpenAISettings> settings,
    ILogger<AiChatService> logger,
    IMemoryCache memoryCache,
    IOptions<CacheSettings> cacheSettings,
    IHttpClientFactory httpClientFactory,
    ApplicationDbContext dbContext) : IAiChatService
{
    private const int MaxContextMessages = 20;

    private readonly AzureOpenAISettings _settings = settings.Value;
    private readonly ILogger<AiChatService> _logger = logger;
    private readonly IMemoryCache _cache = memoryCache;
    private readonly CacheSettings _cacheSettings = cacheSettings.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ApplicationDbContext _dbContext = dbContext;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public async Task<IReadOnlyList<AiChatConversationSummaryDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AiChatConversations
            .AsNoTracking()
            .Where(conversation => conversation.UserId == userId)
            .OrderByDescending(conversation => conversation.UpdatedAtUtc)
            .Select(conversation => new AiChatConversationSummaryDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                UpdatedAtUtc = conversation.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _dbContext.AiChatConversations
            .FirstOrDefaultAsync(item => item.Id == conversationId && item.UserId == userId, cancellationToken) ?? throw new KeyNotFoundException($"Conversation with ID {conversationId} was not found.");

        // Delete all messages in this conversation
        var messages = await _dbContext.AiChatMessages
            .Where(message => message.ConversationId == conversationId)
            .ToListAsync(cancellationToken);

        _dbContext.AiChatMessages.RemoveRange(messages);

        // Delete the conversation
        _dbContext.AiChatConversations.Remove(conversation);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AiChatConversationMessagesDto> GetConversationMessagesAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversationExists = await _dbContext.AiChatConversations
            .AsNoTracking()
            .AnyAsync(item => item.Id == conversationId && item.UserId == userId, cancellationToken);

        if (!conversationExists)
        {
            throw new KeyNotFoundException($"Conversation with ID {conversationId} was not found.");
        }

        var conversation = await _dbContext.AiChatConversations
            .AsNoTracking()
            .FirstAsync(item => item.Id == conversationId && item.UserId == userId, cancellationToken);

        var messages = await _dbContext.AiChatMessages
            .AsNoTracking()
            .Where(message => message.ConversationId == conversationId)
            .OrderBy(message => message.SentAtUtc)
            .Select(message => new AiChatMessageDto
            {
                Id = message.Id,
                Role = message.Role,
                Content = message.Content,
                SentAtUtc = message.SentAtUtc,
            })
            .ToListAsync(cancellationToken);

        return new AiChatConversationMessagesDto
        {
            ConversationId = conversationId,
            Title = conversation.Title,
            UpdatedAtUtc = conversation.UpdatedAtUtc,
            Messages = messages,
        };
    }

    public async Task<AiChatResponseDto> SendMessageAsync(Guid userId, AiChatRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message is required.", nameof(request));
        }

        ValidateConfiguration();

        var conversationId = request.ConversationId ?? Guid.NewGuid();
        var normalizedMessage = request.Message.Trim();
        var now = DateTime.UtcNow;

        var conversationExists = await _dbContext.AiChatConversations
            .AsNoTracking()
            .AnyAsync(item => item.Id == conversationId && item.UserId == userId, cancellationToken);

        AiChatConversation conversation;
        if (!conversationExists)
        {
            conversation = new AiChatConversation
            {
                Id = conversationId,
                UserId = userId,
                Title = BuildConversationTitle(normalizedMessage),
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };

            _dbContext.AiChatConversations.Add(conversation);
        }
        else
        {
            conversation = await _dbContext.AiChatConversations
                .Include(item => item.Messages)
                .FirstAsync(item => item.Id == conversationId && item.UserId == userId, cancellationToken);
        }

        var userMessage = new AiChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = "user",
            Content = normalizedMessage,
            SentAtUtc = now,
        };

        conversation.Messages.Add(userMessage);
        conversation.UpdatedAtUtc = now;

        if (string.IsNullOrWhiteSpace(conversation.Title) || conversation.Title == "New chat")
        {
            conversation.Title = BuildConversationTitle(normalizedMessage);
        }

        await _dbContext.AiChatMessages.AddAsync(userMessage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var historyMessages = await _dbContext.AiChatMessages
            .AsNoTracking()
            .Where(message => message.ConversationId == conversationId)
            .OrderBy(message => message.SentAtUtc)
            .ToListAsync(cancellationToken);

        var modelMessages = BuildModelMessages(historyMessages);
        var cacheKey = ComputeCacheKey(conversationId, modelMessages, _settings.DeploymentName);

        string? answer;
        if (_cache.TryGetValue<string>(cacheKey, out var cachedAnswer))
        {
            answer = cachedAnswer;
        }
        else
        {
            answer = await RequestCompletionAsync(modelMessages, cancellationToken);

            if (!string.IsNullOrWhiteSpace(answer))
            {
                var ttlMinutes = _cacheSettings.DefaultExpirationMinutes > 0 ? _cacheSettings.DefaultExpirationMinutes : 1440;
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ttlMinutes),
                };

                _cache.Set(cacheKey, answer, options);
            }
        }

        if (string.IsNullOrWhiteSpace(answer))
        {
            answer = "I could not generate a response. Please try again.";
        }

        var assistantMessage = new AiChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = "assistant",
            Content = answer,
            SentAtUtc = DateTime.UtcNow,
        };

        conversation.Messages.Add(assistantMessage);
        conversation.UpdatedAtUtc = assistantMessage.SentAtUtc;

        await _dbContext.AiChatMessages.AddAsync(assistantMessage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AiChatResponseDto
        {
            ConversationId = conversationId,
            Answer = answer,
            Model = _settings.DeploymentName,
            GeneratedAtUtc = assistantMessage.SentAtUtc,
            ConversationTitle = conversation.Title,
            ConversationUpdatedAtUtc = conversation.UpdatedAtUtc,
        };
    }

    private async Task<string> RequestCompletionAsync(IReadOnlyList<AzureOpenAIChatMessage> messages, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        var chatRequest = new AzureOpenAIChatRequest
        {
            Messages = messages,
            MaxCompletionTokens = 2000,
        };

        var endpoint = BuildChatCompletionsUri();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(chatRequest, JsonOptions),
                Encoding.UTF8,
                "application/json"),
        };

        httpRequest.Headers.Add("api-key", _settings.ApiKey);

        using var httpResponse = await client.SendAsync(httpRequest, cancellationToken);
        var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Azure OpenAI request failed with status {StatusCode}: {ResponseContent}", (int)httpResponse.StatusCode, responseContent);
            throw new InvalidOperationException($"Azure OpenAI request failed with status {(int)httpResponse.StatusCode}: {responseContent}");
        }

        var aiResponse = JsonSerializer.Deserialize<AzureOpenAIChatResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Azure OpenAI response could not be parsed.");

        var content = aiResponse.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? string.Empty;
        return content;
    }

    private static string ComputeCacheKey(Guid conversationId, IReadOnlyList<AzureOpenAIChatMessage> messages, string deployment)
    {
        var payload = JsonSerializer.Serialize(messages, JsonOptions);
        var input = Encoding.UTF8.GetBytes($"{deployment}|{conversationId}|{payload}");
        var hash = SHA256.HashData(input);
        return "aichat:" + Convert.ToBase64String(hash);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(nameof(AiChatService));
        client.BaseAddress = new Uri(_settings.Endpoint);
        return client;
    }

    private Uri BuildChatCompletionsUri()
    {
        var baseUri = _settings.Endpoint.TrimEnd('/');
        var apiVersion = string.IsNullOrWhiteSpace(_settings.ApiVersion)
            ? "2024-12-01-preview"
            : _settings.ApiVersion.Trim();

        return new Uri($"{baseUri}/openai/deployments/{_settings.DeploymentName}/chat/completions?api-version={Uri.EscapeDataString(apiVersion)}");
    }

    private List<AzureOpenAIChatMessage> BuildModelMessages(IEnumerable<AiChatMessage> historyMessages)
    {
        var messages = new List<AzureOpenAIChatMessage>();

        if (!string.IsNullOrWhiteSpace(_settings.SystemPrompt))
        {
            messages.Add(new()
            {
                Role = "system",
                Content = _settings.SystemPrompt,
            });
        }

        foreach (var message in historyMessages.TakeLast(MaxContextMessages))
        {
            if (string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
            {
                messages.Add(new()
                {
                    Role = message.Role,
                    Content = message.Content,
                });
            }
        }

        return messages;
    }

    private static string BuildConversationTitle(string message)
    {
        var trimmed = message.Trim();
        return trimmed.Length > 28 ? $"{trimmed[..28]}..." : trimmed;
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.Endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Azure OpenAI API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.DeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI deployment name is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiVersion))
        {
            throw new InvalidOperationException("Azure OpenAI API version is not configured.");
        }
    }

    private sealed class AzureOpenAIChatRequest
    {
        [JsonPropertyName("messages")]
        public IReadOnlyList<AzureOpenAIChatMessage> Messages { get; set; } = [];

        [JsonPropertyName("max_completion_tokens")]
        public int MaxCompletionTokens { get; set; }
    }

    private sealed class AzureOpenAIChatResponse
    {
        [JsonPropertyName("choices")]
        public List<AzureOpenAIChoice> Choices { get; set; } = [];
    }

    private sealed class AzureOpenAIChoice
    {
        [JsonPropertyName("message")]
        public AzureOpenAIChatMessage? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        [JsonPropertyName("content_filter_results")]
        public Dictionary<string, object>? ContentFilterResults { get; set; }
    }

    private sealed class AzureOpenAIChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
