using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Comment;

public class CreateCommentRequestDto
{
    [Required(ErrorMessage = "Comment is required")]
    public CreateCommentDto Comment { get; set; } = null!;

    public Guid? ParentId { get; set; }

    public string? Action { get; set; }
}
