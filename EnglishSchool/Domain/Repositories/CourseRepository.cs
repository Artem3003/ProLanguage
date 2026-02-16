using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class CourseRepository(ApplicationDbContext context) : AbstractRepository<Course>(context), ICourseRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Course?> GetByTitleAsync(string title)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.Title == title);
    }

    public async Task<bool> TitleExistsAsync(string title, Guid? excludeId = null)
    {
        return excludeId.HasValue
            ? await _context.Courses
                .AnyAsync(c => c.Title == title && c.Id != excludeId.Value)
            : await _context.Courses
            .AnyAsync(c => c.Title == title);
    }

    public async Task<IEnumerable<Course>> GetAvailableCoursesAsync(Guid? excludeLessonId = null)
    {
        var query = _context.Courses
            .Include(c => c.Lessons)
            .AsQueryable();

        if (excludeLessonId.HasValue)
        {
            // When editing, exclude the current lesson from the count
            return await query
                .Where(c => c.Lessons.Count(l => l.Id != excludeLessonId.Value) < c.NumberOfLessons)
                .ToListAsync();
        }

        return await query
            .Where(c => c.Lessons.Count < c.NumberOfLessons)
            .ToListAsync();
    }

    public IQueryable<Course> GetQueryable()
    {
        return _context.Courses.AsQueryable();
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            course.ViewCount++;
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }
    }
}
