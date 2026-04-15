using Domain.Entities.Enums;

namespace Application.DTOs.CalendarEvent;

public class UpdateCalendarEventDto
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public EventType? Type { get; set; }

    public string? Color { get; set; }

    public Guid? LessonId { get; set; }
}
