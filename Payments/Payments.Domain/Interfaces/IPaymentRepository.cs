using Payments.Domain.Entities;

namespace Payments.Domain.Interfaces;

/// <summary>
/// Repository interface for payment operations.
/// </summary>
public interface IPaymentRepository : IRepository<Payment>
{
    /// <summary>
    /// Gets payments by user identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A collection of payments for the user.</returns>
    Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets payments by course identifier.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>A collection of payments for the course.</returns>
    Task<IEnumerable<Payment>> GetPaymentsByCourseIdAsync(Guid courseId);

    /// <summary>
    /// Gets a payment with its transactions.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <returns>The payment with transactions or null if not found.</returns>
    Task<Payment?> GetPaymentWithTransactionsAsync(Guid paymentId);

    /// <summary>
    /// Gets payments by status.
    /// </summary>
    /// <param name="status">The payment status.</param>
    /// <returns>A collection of payments with the specified status.</returns>
    Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status);
}
