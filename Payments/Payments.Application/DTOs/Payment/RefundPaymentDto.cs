namespace Payments.Application.DTOs.Payment;

/// <summary>
/// Data transfer object for refunding a payment.
/// </summary>
public class RefundPaymentDto
{
    /// <summary>
    /// Gets or sets the payment identifier.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the refund amount. If null, full amount is refunded.
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Gets or sets the refund reason.
    /// </summary>
    public string? Reason { get; set; }
}
