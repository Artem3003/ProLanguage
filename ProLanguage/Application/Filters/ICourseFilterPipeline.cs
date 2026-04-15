using Application.DTOs.Course;
using Domain.Entities;

namespace Application.Filters;

/// <summary>
/// Interface for the course filter pipeline that combines filtering, sorting, and pagination.
/// </summary>
public interface ICourseFilterPipeline
{
    /// <summary>
    /// Executes the filter pipeline on the query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>Tuple containing the paginated query and total count before pagination.</returns>
    (IQueryable<Course> Query, int TotalCount) Execute(IQueryable<Course> query, CourseFilterDto filter);
}
