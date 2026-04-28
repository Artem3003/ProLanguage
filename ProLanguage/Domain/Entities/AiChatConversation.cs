using Domain.Entities.Common;

namespace Domain.Entities;

public class AiChatConversation : BaseEntity<Guid>
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = "New chat";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<AiChatMessage> Messages { get; set; } = [];
}