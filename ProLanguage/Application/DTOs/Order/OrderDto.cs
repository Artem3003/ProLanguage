namespace Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public DateTime? Date { get; set; }

    public string? Status { get; set; }
}
