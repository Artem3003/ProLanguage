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

    public (IQueryable<Course> Query, int TotalCount) Execute(IQueryable<Course> query, CourseFilterDto filter)
    {
        // Step 1: Apply filters
        var filteredQuery = _filter.Apply(query, filter);

        // Step 2: Get total count before pagination
        var totalCount = filteredQuery.Count();

        // Step 3: Apply sorting
        var sortedQuery = _sorter.Apply(filteredQuery, filter.SortBy);

        // Step 4: Apply pagination
        var paginatedQuery = _paginator.Apply(sortedQuery, filter.PageSize, filter.Page);

        return (paginatedQuery, totalCount);
    }
}
