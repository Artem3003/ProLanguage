using Application.DTOs.Course;
using Domain.Entities;

namespace Application.Filters;

/// <summary>
/// Implementation of the course filter pipeline that combines filtering, sorting, and pagination.
/// </summary>
public class CourseFilterPipeline(
    ICourseFilter filter,
    ICourseSorter sorter,
    ICoursePaginator paginator) : ICourseFilterPipeline
{
    private readonly ICourseFilter _filter = filter;
    private readonly ICourseSorter _sorter = sorter;
    private readonly ICoursePaginator _paginator = paginator;

    public async Task<(IQueryable<Course> Query, int TotalCount)> ExecuteAsync(IQueryable<Course> query, CourseFilterDto filter)
    {
        // Step 1: Apply filters
        var filteredQuery = _filter.Apply(query, filter);

        // Step 2: Get total count before pagination (async)
        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(filteredQuery);

        // Step 3: Apply sorting
        var sortedQuery = _sorter.Apply(filteredQuery, filter.SortBy);

        // Step 4: Apply pagination
        var paginatedQuery = _paginator.Apply(sortedQuery, filter.PageSize, filter.Page);

        return (paginatedQuery, totalCount);
    }
}
