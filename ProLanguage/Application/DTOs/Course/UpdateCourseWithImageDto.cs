using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Course;

public class UpdateCourseWithImageDto
{
    [Required(ErrorMessage = "Course is required")]
    public UpdateCourseDto Course { get; set; } = new();

    public string? Image { get; set; }
}
