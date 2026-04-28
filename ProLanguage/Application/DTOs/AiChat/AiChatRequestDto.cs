namespace Application.DTOs.AiChat;

public class AiChatRequestDto
{
    public string Message { get; set; } = string.Empty;

    public Guid? ConversationId { get; set; }
}
