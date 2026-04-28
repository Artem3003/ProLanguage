namespace Application.DTOs.AiChat;

public class AiChatResponseDto
{
    public Guid ConversationId { get; set; }

    public string Answer { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; }

    public string ConversationTitle { get; set; } = string.Empty;

    public DateTime ConversationUpdatedAtUtc { get; set; }
}
