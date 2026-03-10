namespace Application.DTOs.Order;

public class PaymentRequestDto
{
    public string Method { get; set; } = string.Empty;

    public VisaPaymentModelDto? Model { get; set; }
}
