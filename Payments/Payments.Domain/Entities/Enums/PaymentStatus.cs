namespace Payments.Domain.Entities.Enums;

/// <summary>
/// Represents the status of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment is being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Payment has been completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Payment has failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Payment has been refunded.
    /// </summary>
    Refunded = 4,

    /// <summary>
    /// Payment has been cancelled.
    /// </summary>
    Cancelled = 5
}
