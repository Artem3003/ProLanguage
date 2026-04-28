using Application.DTOs.AiChat;

namespace Application.Interfaces;

public interface IAiChatService
{
    Task<IReadOnlyList<AiChatConversationSummaryDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AiChatConversationMessagesDto> GetConversationMessagesAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);

    Task<AiChatResponseDto> SendMessageAsync(Guid userId, AiChatRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
}
