using Domain.Entities;

namespace Application.Filters;

/// <summary>
/// Interface for applying pagination to course queries.
/// </summary>
public interface ICoursePaginator
{
    /// <summary>
    /// Applies pagination to the query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="page">The current page number (1-based).</param>
    /// <returns>The paginated query.</returns>
    IQueryable<Course> Apply(IQueryable<Course> query, int pageSize, int page);

    /// <summary>
    /// Calculates total pages based on total count and page size.
    /// </summary>
    /// <param name="totalCount">Total number of items.</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>Total number of pages.</returns>
    int CalculateTotalPages(int totalCount, int pageSize);
}
