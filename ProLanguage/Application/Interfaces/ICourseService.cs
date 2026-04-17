using Application.DTOs.Course;
using Domain.Entities.Enums;

namespace Application.Interfaces;

public interface ICourseService
{
    Task<Guid> CreateCourseAsync(CreateCourseDto dto);

    Task<CourseDto> GetCourseByIdAsync(Guid id);

    Task<CourseDto?> GetCourseByTitleAsync(string title);

    Task<IEnumerable<CourseDto>> GetAllCoursesAsync();

    Task<IEnumerable<CourseDto>> GetAvailableCoursesAsync(Guid? excludeLessonId = null);

    Task UpdateCourseAsync(UpdateCourseDto dto);

    Task DeleteCourseAsync(Guid id);

    Task<CourseImageDto?> GetCourseImageAsync(Guid id);

    Task RemoveCourseImageAsync(Guid id);

    /// <summary>
    /// Gets filtered, sorted, and paginated courses.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>Paginated course results.</returns>
    Task<CourseFilterResultDto> GetFilteredCoursesAsync(CourseFilterDto filter);

    /// <summary>
    /// Gets course by ID and increments view count.
    /// </summary>
    /// <param name="id">Course ID.</param>
    /// <returns>Course DTO.</returns>
    Task<CourseDto> GetCourseDetailAsync(Guid id);

    /// <summary>
    /// Gets available pagination options.
    /// </summary>
    /// <returns>List of page size options.</returns>
    List<int> GetPaginationOptions();

    /// <summary>
    /// Gets available sorting options.
    /// </summary>
    /// <returns>List of sorting option key-value pairs.</returns>
    List<KeyValuePair<string, string>> GetSortingOptions();

    /// <summary>
    /// Gets available languages for filtering.
    /// </summary>
    /// <returns>List of language options.</returns>
    List<KeyValuePair<CourseLanguage, string>> GetLanguages();

    /// <summary>
    /// Gets available levels for filtering.
    /// </summary>
    /// <returns>List of level options.</returns>
    List<KeyValuePair<CourseLevel, string>> GetLevels();

    /// <summary>
    /// Gets available rating options for filtering.
    /// </summary>
    /// <returns>List of rating options.</returns>
    List<KeyValuePair<double, string>> GetRatingOptions();
}
