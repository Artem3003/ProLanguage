using Payments.Domain.Entities.Common;
using Payments.Domain.Entities.Enums;

namespace Payments.Domain.Entities;

/// <summary>
/// Represents a payment entity.
/// </summary>
public class Payment : BaseEntity<Guid>
{
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
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the payment status.
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the failure reason if payment failed.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway response.
    /// </summary>
    public string? PaymentGatewayResponse { get; set; }

    /// <summary>
    /// Gets or sets the collection of transactions.
    /// </summary>
    public List<Transaction> Transactions { get; set; } = [];
}
