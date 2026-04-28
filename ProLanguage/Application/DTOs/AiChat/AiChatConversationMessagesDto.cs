namespace Application.DTOs.AiChat;

public class AiChatConversationMessagesDto
{
    public Guid ConversationId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }

    public List<AiChatMessageDto> Messages { get; set; } = [];
}