using Payments.Domain.Entities.Enums;

namespace Payments.Application.DTOs.Payment;

/// <summary>
/// Data transfer object for updating payment status.
/// </summary>
public class UpdatePaymentStatusDto
{
    /// <summary>
    /// Gets or sets the payment identifier.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the new payment status.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the failure reason.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway response.
    /// </summary>
    public string? PaymentGatewayResponse { get; set; }
}
