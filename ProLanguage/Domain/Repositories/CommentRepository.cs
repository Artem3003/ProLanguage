using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class CommentRepository(ApplicationDbContext context) : AbstractRepository<Comment>(context), ICommentRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Comment>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.Comments
            .Where(c => c.CourseId == courseId && c.ParentCommentId == null)
            .Include(c => c.ChildComments)
            .ToListAsync();
    }

    public async Task<Comment?> GetByIdWithChildrenAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.ChildComments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
