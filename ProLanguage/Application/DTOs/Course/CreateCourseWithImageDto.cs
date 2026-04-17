using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Course;

public class CreateCourseWithImageDto
{
    [Required(ErrorMessage = "Course is required")]
    public CreateCourseDto Course { get; set; } = new();

    public string? Image { get; set; }
}
