using Microsoft.EntityFrameworkCore.Storage;
using Payments.Domain.Interfaces;

namespace Payments.Domain.Data;

/// <summary>
/// Unit of work implementation for managing database transactions.
/// </summary>
/// <param name="context">The payments database context.</param>
public class UnitOfWork(PaymentsDbContext context) : IUnitOfWork
{
    private readonly PaymentsDbContext _context = context;
    private IDbContextTransaction? _transaction;

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
