using Payments.Application.DTOs.Transaction;

namespace Payments.Application.Interfaces;

/// <summary>
/// Service interface for transaction operations.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Creates a new transaction asynchronously.
    /// </summary>
    /// <param name="createTransactionDto">The transaction creation data.</param>
    /// <returns>The created transaction.</returns>
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto);

    /// <summary>
    /// Gets a transaction by identifier asynchronously.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <returns>The transaction or null if not found.</returns>
    Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId);

    /// <summary>
    /// Gets transactions by payment identifier asynchronously.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <returns>A collection of transactions for the payment.</returns>
    Task<IEnumerable<TransactionDto>> GetTransactionsByPaymentIdAsync(Guid paymentId);

    /// <summary>
    /// Gets all transactions asynchronously.
    /// </summary>
    /// <returns>A collection of all transactions.</returns>
    Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();
}
