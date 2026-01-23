using Payments.Application.DTOs.Payment;

namespace Payments.Application.Interfaces;

/// <summary>
/// Service interface for payment operations.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a new payment asynchronously.
    /// </summary>
    /// <param name="createPaymentDto">The payment creation data.</param>
    /// <returns>The created payment.</returns>
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto);

    /// <summary>
    /// Gets a payment by identifier asynchronously.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <returns>The payment or null if not found.</returns>
    Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId);

    /// <summary>
    /// Gets payments by user identifier asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A collection of payments for the user.</returns>
    Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets payments by course identifier asynchronously.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>A collection of payments for the course.</returns>
    Task<IEnumerable<PaymentDto>> GetPaymentsByCourseIdAsync(Guid courseId);

    /// <summary>
    /// Processes a payment asynchronously.
    /// </summary>
    /// <param name="processPaymentDto">The payment processing data.</param>
    /// <returns>The processed payment.</returns>
    Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto processPaymentDto);

    /// <summary>
    /// Updates the payment status asynchronously.
    /// </summary>
    /// <param name="updateStatusDto">The status update data.</param>
    /// <returns>The updated payment.</returns>
    Task<PaymentDto> UpdatePaymentStatusAsync(UpdatePaymentStatusDto updateStatusDto);

    /// <summary>
    /// Refunds a payment asynchronously.
    /// </summary>
    /// <param name="refundDto">The refund data.</param>
    /// <returns>The refunded payment.</returns>
    Task<PaymentDto> RefundPaymentAsync(RefundPaymentDto refundDto);

    /// <summary>
    /// Gets all payments asynchronously.
    /// </summary>
    /// <returns>A collection of all payments.</returns>
    Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();

    /// <summary>
    /// Deletes a payment asynchronously.
    /// </summary>
    /// <param name="paymentId">The payment identifier.</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    Task<bool> DeletePaymentAsync(Guid paymentId);
}
