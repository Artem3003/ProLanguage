using Domain.Entities.Common;

namespace Domain.Entities;

public class Homework : BaseEntity<Guid>
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? LessonId { get; set; }

    public Lesson? Lesson { get; set; }

    public List<HomeworkAssignment> HomeworkAssignments { get; set; } = [];
}
