using Application.DTOs.Course;
using Domain.Entities;

namespace Application.Filters;

/// <summary>
/// Interface for applying filter criteria to course queries.
/// </summary>
public interface ICourseFilter
{
    /// <summary>
    /// Applies filter criteria to the query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>The filtered query.</returns>
    IQueryable<Course> Apply(IQueryable<Course> query, CourseFilterDto filter);
}
