using Microsoft.EntityFrameworkCore;
using Payments.Domain.Data;
using Payments.Domain.Entities;
using Payments.Domain.Interfaces;

namespace Payments.Domain.Repositories;

/// <summary>
/// Repository implementation for transaction operations.
/// </summary>
/// <param name="context">The payments database context.</param>
public class TransactionRepository(PaymentsDbContext context) : AbstractRepository<Transaction>(context), ITransactionRepository
{
    private readonly PaymentsDbContext _context = context;

    /// <inheritdoc/>
    public async Task<IEnumerable<Transaction>> GetTransactionsByPaymentIdAsync(Guid paymentId)
    {
        return await _context.Transactions
            .Where(t => t.PaymentId == paymentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
