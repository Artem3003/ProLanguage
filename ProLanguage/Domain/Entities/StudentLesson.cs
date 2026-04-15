using Domain.Entities.Common;
using Domain.Entities.Enums;

namespace Domain.Entities;

public class StudentLesson : BaseEntity<Guid>
{
    public Guid LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public AttendanceStatus AttendanceStatus { get; set; }

    public string? Notes { get; set; }

    public DateTime? AttendedAt { get; set; }
}
