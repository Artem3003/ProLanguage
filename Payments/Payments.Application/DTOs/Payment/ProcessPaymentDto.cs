namespace Payments.Application.DTOs.Payment;

/// <summary>
/// Data transfer object for processing a payment.
/// </summary>
public class ProcessPaymentDto
{
    /// <summary>
    /// Gets or sets the payment identifier.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the payment method token.
    /// </summary>
    public string? PaymentMethodToken { get; set; }

    /// <summary>
    /// Gets or sets additional data for payment processing.
    /// </summary>
    public Dictionary<string, string>? AdditionalData { get; set; }
}
