using Domain.Entities;

namespace Domain.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> GetFlatByCourseIdAsync(Guid courseId);

    Task<Comment?> GetByIdWithParentAsync(Guid id);
}
