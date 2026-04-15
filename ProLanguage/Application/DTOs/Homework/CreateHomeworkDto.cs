namespace Application.DTOs.Homework;

public class CreateHomeworkDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public Guid? LessonId { get; set; }
}
