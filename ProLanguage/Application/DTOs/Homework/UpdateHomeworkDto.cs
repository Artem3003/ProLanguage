namespace Application.DTOs.Homework;

public class UpdateHomeworkDto
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Instructions { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid? LessonId { get; set; }
}
