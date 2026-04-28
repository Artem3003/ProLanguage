namespace Application.DTOs.AiChat;

public class AiChatConversationSummaryDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }
}