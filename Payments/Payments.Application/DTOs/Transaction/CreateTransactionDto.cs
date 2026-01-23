namespace Payments.Application.DTOs.Transaction;

/// <summary>
/// Data transfer object for creating a transaction.
/// </summary>
public class CreateTransactionDto
{
    /// <summary>
    /// Gets or sets the payment identifier.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the transaction type (Charge, Refund, Authorization).
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
    /// Gets or sets the gateway response.
    /// </summary>
    public string? GatewayResponse { get; set; }

    /// <summary>
    /// Gets or sets the transaction notes.
    /// </summary>
    public string? Notes { get; set; }
}
