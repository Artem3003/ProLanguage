using Payments.Domain.Entities.Enums;

namespace Payments.Application.DTOs.Payment;

/// <summary>
/// Data transfer object for payment information.
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// Gets or sets the payment identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the course identifier.
    /// </summary>
    public Guid? CourseId { get; set; }

    /// <summary>
    /// Gets or sets the payment amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the payment status.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the payment description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the failure reason if payment failed.
    /// </summary>
    public string? FailureReason { get; set; }
}
