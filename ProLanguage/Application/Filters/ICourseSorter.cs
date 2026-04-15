using Domain.Entities;

namespace Application.Filters;

/// <summary>
/// Interface for applying sorting to course queries.
/// </summary>
public interface ICourseSorter
{
    /// <summary>
    /// Applies sorting to the query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="sortBy">The sort criteria (e.g., "price_asc", "rating_desc", "popularity", "newest").</param>
    /// <returns>The sorted query.</returns>
    IQueryable<Course> Apply(IQueryable<Course> query, string sortBy);
}
