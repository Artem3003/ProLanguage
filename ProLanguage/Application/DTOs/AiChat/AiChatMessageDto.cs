namespace Application.DTOs.AiChat;

public class AiChatMessageDto
{
    public Guid Id { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime SentAtUtc { get; set; }
}