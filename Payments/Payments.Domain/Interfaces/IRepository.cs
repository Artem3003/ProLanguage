using System.Linq.Expressions;
using Payments.Domain.Entities.Common;

namespace Payments.Domain.Interfaces;

/// <summary>
/// Generic repository interface for entity operations.
/// </summary>
/// <typeparam name="TEntity">The type of entity.</typeparam>
public interface IRepository<TEntity>
    where TEntity : BaseEntity<Guid>
{
    /// <summary>
    /// Adds an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Updates an entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Gets an entity by identifier asynchronously.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The entity or null if not found.</returns>
    Task<TEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all entities asynchronously.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Finds entities matching a predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>A collection of matching entities.</returns>
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
}
