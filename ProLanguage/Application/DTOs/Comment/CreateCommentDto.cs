using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Comment;

public class CreateCommentDto
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Body is required")]
    public string Body { get; set; } = string.Empty;
}
