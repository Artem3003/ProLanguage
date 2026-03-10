using Domain.Entities;

namespace Application.Filters;

/// <summary>
/// Implementation of course sorting logic.
/// </summary>
public class CourseSorter : ICourseSorter
{
    public IQueryable<Course> Apply(IQueryable<Course> query, string sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(c => c.IsOnSale && c.DiscountPrice.HasValue ? c.DiscountPrice.Value : c.Price),
            "price_desc" => query.OrderByDescending(c => c.IsOnSale && c.DiscountPrice.HasValue ? c.DiscountPrice.Value : c.Price),
            "rating_asc" => query.OrderBy(c => c.Rating),
            "rating_desc" => query.OrderByDescending(c => c.Rating),
            "duration_asc" => query.OrderBy(c => c.DurationMinutes),
            "duration_desc" => query.OrderByDescending(c => c.DurationMinutes),
            "title_asc" => query.OrderBy(c => c.Title),
            "title_desc" => query.OrderByDescending(c => c.Title),
            "popularity" => query.OrderByDescending(c => c.ViewCount),
            "newest" => query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt), // Default: newest first
        };
    }
}
