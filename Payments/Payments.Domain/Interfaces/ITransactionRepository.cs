using Payments.Domain.Entities;

namespace Payments.Domain.Interfaces;

/// <summary>
/// Repository interface for transaction operations.
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Gets transactions by payment identifier.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <returns>A collection of transactions for the payment.</returns>
    Task<IEnumerable<Transaction>> GetTransactionsByPaymentIdAsync(Guid paymentId);
}
