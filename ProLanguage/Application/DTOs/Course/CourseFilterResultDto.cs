namespace Application.DTOs.Course;

/// <summary>
/// DTO for paginated course filter results.
/// </summary>
public class CourseFilterResultDto
{
    /// <summary>
    /// Gets or sets the list of courses matching the filter criteria.
    /// </summary>
    public List<CourseDto> Courses { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the total number of courses matching the filter criteria.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page size.
    /// </summary>
    public int PageSize { get; set; }
}
