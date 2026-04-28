using Domain.Entities.Common;

namespace Domain.Entities;

public class AiChatMessage : BaseEntity<Guid>
{
    public Guid ConversationId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

    public AiChatConversation? Conversation { get; set; }
}