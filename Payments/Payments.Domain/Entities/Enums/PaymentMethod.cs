namespace Payments.Domain.Entities.Enums;

/// <summary>
/// Represents the available payment methods.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Credit card payment.
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// Debit card payment.
    /// </summary>
    DebitCard = 1,

    /// <summary>
    /// PayPal payment.
    /// </summary>
    PayPal = 2,

    /// <summary>
    /// Bank transfer payment.
    /// </summary>
    BankTransfer = 3,

    /// <summary>
    /// Stripe payment.
    /// </summary>
    Stripe = 4,

    /// <summary>
    /// Cash payment.
    /// </summary>
    Cash = 5
}
