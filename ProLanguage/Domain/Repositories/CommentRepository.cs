using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class CommentRepository(ApplicationDbContext context) : AbstractRepository<Comment>(context), ICommentRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Comment>> GetFlatByCourseIdAsync(Guid courseId)
    {
        return await _context.Comments
            .Where(c => c.CourseId == courseId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetByIdWithParentAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.ParentComment)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
