using System.Linq.Expressions;
using Domain.Data;
using Domain.Entities.Common;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public abstract class AbstractRepository<TEntity>(ApplicationDbContext context) : IRepository<TEntity>
    where TEntity : BaseEntity<Guid>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var entry = await _context.AddAsync(entity);
        return entry.Entity;
    }

    public void Delete(TEntity entity)
    {
        _context.Remove(entity);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        var entity = await _context.FindAsync<TEntity>(id);
        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }

    public void Update(TEntity entity)
    {
        _context.Update(entity);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().Where(predicate).ToListAsync();
    }
}
