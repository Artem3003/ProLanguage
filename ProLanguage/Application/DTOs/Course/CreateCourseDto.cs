using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Course;

public class CreateCourseDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    public double Price { get; set; }

    [Required(ErrorMessage = "Number of lessons is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Number of lessons must be at least 1")]
    public int NumberOfLessons { get; set; }

    [Required(ErrorMessage = "Language is required")]
    public CourseLanguage Language { get; set; }

    [Required(ErrorMessage = "Level is required")]
    public CourseLevel Level { get; set; }

    [Required(ErrorMessage = "Duration is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 minute")]
    public int DurationMinutes { get; set; }

    public bool IsOnSale { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Discount price must be a positive number")]
    public double? DiscountPrice { get; set; }

    public string? Image { get; set; }
}
