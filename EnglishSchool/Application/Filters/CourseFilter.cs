using Application.DTOs.Course;
using Domain.Entities;

namespace Application.Filters;

/// <summary>
/// Implementation of course filtering logic.
/// </summary>
public class CourseFilter : ICourseFilter
{
    public IQueryable<Course> Apply(IQueryable<Course> query, CourseFilterDto filter)
    {
        // Filter by new (created within last 30 days)
        if (filter.IsNew == true)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            query = query.Where(c => c.CreatedAt >= thirtyDaysAgo);
        }

        // Filter by languages
        if (filter.Languages != null && filter.Languages.Count > 0)
        {
            query = query.Where(c => filter.Languages.Contains(c.Language));
        }

        // Filter by price range
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(c => c.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= filter.MaxPrice.Value);
        }

        // Filter by levels
        if (filter.Levels != null && filter.Levels.Count > 0)
        {
            query = query.Where(c => filter.Levels.Contains(c.Level));
        }

        // Filter by on sale
        if (filter.OnSale == true)
        {
            query = query.Where(c => c.IsOnSale);
        }

        // Filter by minimum rating
        if (filter.MinRating.HasValue)
        {
            query = query.Where(c => c.Rating >= filter.MinRating.Value);
        }

        // Filter by duration range
        if (filter.MinDuration.HasValue)
        {
            query = query.Where(c => c.DurationMinutes >= filter.MinDuration.Value);
        }

        if (filter.MaxDuration.HasValue)
        {
            query = query.Where(c => c.DurationMinutes <= filter.MaxDuration.Value);
        }

        // Filter by title (partial match)
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            var searchTerm = filter.Title;
            query = query.Where(c => c.Title.Contains(searchTerm));
        }

        return query;
    }
}
