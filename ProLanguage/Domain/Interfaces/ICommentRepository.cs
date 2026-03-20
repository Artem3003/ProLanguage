using Domain.Entities;

namespace Domain.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByCourseIdAsync(Guid courseId);

    Task<Comment?> GetByIdWithChildrenAsync(Guid id);
}
