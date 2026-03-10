using Domain.Entities.Enums;

namespace Application.DTOs.Lesson;

public class UpdateLessonDto
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? DurationMinutes { get; set; }

    public LessonType? Type { get; set; }

    public LessonStatus? Status { get; set; }

    public string? MeetingLink { get; set; }

    public string? Materials { get; set; }

    public Guid? CourseId { get; set; }
}
