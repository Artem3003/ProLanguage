using Domain.Entities;

namespace Application.Filters;

/// <summary>
/// Implementation of course pagination logic.
/// </summary>
public class CoursePaginator : ICoursePaginator
{
    private static readonly int[] ValidPageSizes = [9, 18, 36, 72];

    public IQueryable<Course> Apply(IQueryable<Course> query, int pageSize, int page)
    {
        // Validate and normalize page size
        var normalizedPageSize = ValidPageSizes.Contains(pageSize) ? pageSize : 9;

        // Validate page number
        var normalizedPage = page < 1 ? 1 : page;

        var skip = (normalizedPage - 1) * normalizedPageSize;
        return query.Skip(skip).Take(normalizedPageSize);
    }

    public int CalculateTotalPages(int totalCount, int pageSize)
    {
        var normalizedPageSize = ValidPageSizes.Contains(pageSize) ? pageSize : 9;
        return (int)Math.Ceiling((double)totalCount / normalizedPageSize);
    }
}
