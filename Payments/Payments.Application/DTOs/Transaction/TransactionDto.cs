namespace Payments.Application.DTOs.Transaction;

/// <summary>
/// Data transfer object for transaction information.
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the payment identifier.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the transaction type.
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the transaction status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gateway transaction identifier.
    /// </summary>
    public string? GatewayTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the transaction notes.
    /// </summary>
    public string? Notes { get; set; }
}
