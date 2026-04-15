using Domain.Entities.Enums;

namespace Application.DTOs.Course;

/// <summary>
/// DTO containing all available filter options for the course catalog.
/// </summary>
public class CourseFilterOptionsDto
{
    /// <summary>
    /// Gets or sets the available page size options.
    /// </summary>
    public List<int> PaginationOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the available sorting options (key = sort value, value = display name).
    /// </summary>
    public List<KeyValuePair<string, string>> SortingOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the available language options.
    /// </summary>
    public List<KeyValuePair<CourseLanguage, string>> Languages { get; set; } = [];

    /// <summary>
    /// Gets or sets the available level options.
    /// </summary>
    public List<KeyValuePair<CourseLevel, string>> Levels { get; set; } = [];

    /// <summary>
    /// Gets or sets the available rating filter options.
    /// </summary>
    public List<KeyValuePair<double, string>> RatingOptions { get; set; } = [];
}
