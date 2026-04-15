using Domain.Entities;

namespace Domain.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<Course?> GetByTitleAsync(string title);

    Task<bool> TitleExistsAsync(string title, Guid? excludeId = null);

    Task<IEnumerable<Course>> GetAvailableCoursesAsync(Guid? excludeLessonId = null);

    IQueryable<Course> GetQueryable();

    Task IncrementViewCountAsync(Guid id);
}
