using Domain.Entities.Common;

namespace Domain.Interfaces;

public interface IRepository<TEntity>
    where TEntity : BaseEntity<Guid>
{
    Task<TEntity?> GetByIdAsync(Guid id);

    Task<IEnumerable<TEntity>> GetAllAsync();

    Task<TEntity> AddAsync(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);
}
