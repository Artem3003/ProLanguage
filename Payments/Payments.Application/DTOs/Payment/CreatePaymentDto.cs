using Payments.Domain.Entities.Enums;

namespace Payments.Application.DTOs.Payment;

/// <summary>
/// Data transfer object for creating a payment.
/// </summary>
public class CreatePaymentDto
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
    /// Gets or sets the payment description.
    /// </summary>
    public string? Description { get; set; }
}
