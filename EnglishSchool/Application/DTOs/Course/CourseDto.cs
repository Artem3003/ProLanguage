using Domain.Entities.Enums;

namespace Application.DTOs.Course;

public class CourseDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Price { get; set; }

    public int NumberOfLessons { get; set; }

    public CourseLanguage Language { get; set; }

    public CourseLevel Level { get; set; }

    public int DurationMinutes { get; set; }

    public string DurationFormatted => DurationMinutes >= 60
        ? $"{DurationMinutes / 60}h {DurationMinutes % 60}m"
        : $"{DurationMinutes}m";

    public double Rating { get; set; }

    public bool IsOnSale { get; set; }

    public double? DiscountPrice { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsNew => CreatedAt >= DateTime.UtcNow.AddDays(-30);

    public string? ImageUrl { get; set; }
}
