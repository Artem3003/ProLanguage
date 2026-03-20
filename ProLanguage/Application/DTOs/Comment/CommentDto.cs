namespace Application.DTOs.Comment;

public class CommentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public List<CommentDto> ChildComments { get; set; } = [];
}
