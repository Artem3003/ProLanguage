using Domain.Entities.Enums;

namespace Application.DTOs.Lesson;

public class LessonDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public LessonType Type { get; set; }

    public LessonStatus Status { get; set; }

    public string? MeetingLink { get; set; }

    public string? Materials { get; set; }

    public Guid? CourseId { get; set; }

    public string? CourseTitle { get; set; }

    public DateTime CreatedAt { get; set; }
}
