using Domain.Entities.Common;

namespace Domain.Entities;

public class Comment : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }

    public Guid CourseId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Comment? ParentComment { get; set; }

    public List<Comment> ChildComments { get; set; } = [];

    public Course? Course { get; set; }
}
