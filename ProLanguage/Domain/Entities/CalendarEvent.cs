using Domain.Entities.Common;
using Domain.Entities.Enums;

namespace Domain.Entities;

public class CalendarEvent : BaseEntity<Guid>
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public EventType Type { get; set; }

    public string? Color { get; set; }

    public Guid? LessonId { get; set; }

    public Lesson? Lesson { get; set; }
}
