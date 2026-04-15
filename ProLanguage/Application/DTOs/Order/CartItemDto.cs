namespace Application.DTOs.Order;

public class CartItemDto
{
    public Guid CourseId { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public int Discount { get; set; }
}
