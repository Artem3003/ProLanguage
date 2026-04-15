using Domain.Entities.Common;

namespace Domain.Entities;

public class Ban : BaseEntity<Guid>
{
    public string UserName { get; set; } = string.Empty;

    public string Duration { get; set; } = string.Empty;

    public DateTime BannedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }
}
