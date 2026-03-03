using Domain.Entities.Common;
using Domain.Entities.Enums;

namespace Domain.Entities;

public class Course : BaseEntity<Guid>
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Price { get; set; }

    public int NumberOfLessons { get; set; }

    public CourseLanguage Language { get; set; } = CourseLanguage.English;

    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    public int DurationMinutes { get; set; }

    public double? Rating { get; set; }

    public bool IsOnSale { get; set; }

    public double? DiscountPrice { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? ImageUrl { get; set; }

    public List<Lesson> Lessons { get; set; } = [];
}
