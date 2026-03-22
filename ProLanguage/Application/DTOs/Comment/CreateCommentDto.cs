using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Comment;

public class CreateCommentDto
{
    [Required(ErrorMessage = "Body is required")]
    public string Body { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
}
