using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class LessonRepository(ApplicationDbContext context) : AbstractRepository<Lesson>(context), ILessonRepository
{
    private readonly ApplicationDbContext _context = context;

    public override async Task<Lesson?> GetByIdAsync(Guid id)
    {
        return await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public override async Task<IEnumerable<Lesson>> GetAllAsync()
    {
        return await _context.Lessons
            .Include(l => l.Course)
            .ToListAsync();
    }

    public async Task<int> CountLessonsAsync()
    {
        return await _context.Lessons.CountAsync();
    }
}
