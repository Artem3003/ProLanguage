using Domain.Entities.Enums;

namespace Application.DTOs.Course;

/// <summary>
/// DTO for filtering, sorting, and paginating courses.
/// </summary>
public class CourseFilterDto
{
    /// <summary>
    /// Gets or sets a value indicating whether to filter by new courses (created within last 30 days).
    /// </summary>
    public bool? IsNew { get; set; }

    /// <summary>
    /// Gets or sets the languages to filter by.
    /// </summary>
    public List<CourseLanguage>? Languages { get; set; }

    /// <summary>
    /// Gets or sets the minimum price filter value.
    /// </summary>
    public double? MinPrice { get; set; }

    /// <summary>
    /// Gets or sets the maximum price filter value.
    /// </summary>
    public double? MaxPrice { get; set; }

    /// <summary>
    /// Gets or sets the levels to filter by.
    /// </summary>
    public List<CourseLevel>? Levels { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to filter by courses on sale.
    /// </summary>
    public bool? OnSale { get; set; }

    /// <summary>
    /// Gets or sets the minimum rating filter value.
    /// </summary>
    public double? MinRating { get; set; }

    /// <summary>
    /// Gets or sets the minimum duration filter value in minutes.
    /// </summary>
    public int? MinDuration { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration filter value in minutes.
    /// </summary>
    public int? MaxDuration { get; set; }

    /// <summary>
    /// Gets or sets the title search term (partial match).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the sort field and direction (e.g., "price_asc", "rating_desc", "popularity", "newest").
    /// </summary>
    public string SortBy { get; set; } = "newest";

    /// <summary>
    /// Gets or sets the page size for pagination.
    /// </summary>
    public int PageSize { get; set; } = 9;

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;
}
