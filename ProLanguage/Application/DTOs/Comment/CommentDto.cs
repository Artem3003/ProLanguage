namespace Application.DTOs.Comment;

public class CommentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int Rating { get; set; }

    public string Body { get; set; } = string.Empty;

    public bool IsOwnComment { get; set; }

    public List<CommentDto> ChildComments { get; set; } = [];
}
