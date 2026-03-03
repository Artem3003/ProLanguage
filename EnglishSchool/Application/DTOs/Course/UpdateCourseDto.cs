using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Course;

public class UpdateCourseDto
{
    [Required(ErrorMessage = "Id is required")]
    public Guid Id { get; set; }

    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    public double? Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Number of lessons must be at least 1")]
    public int? NumberOfLessons { get; set; }

    public CourseLanguage? Language { get; set; }

    public CourseLevel? Level { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 minute")]
    public int? DurationMinutes { get; set; }

    [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
    public double? Rating { get; set; }

    public bool? IsOnSale { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Discount price must be a positive number")]
    public double? DiscountPrice { get; set; }

    public string? ImageUrl { get; set; }
}
