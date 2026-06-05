using Application.DTOs.Order;

namespace Application.Interfaces;

public interface IOrderService
{
    // Cart operations
    Task AddToCartAsync(Guid customerId, Guid courseId);

    Task RemoveFromCartAsync(Guid customerId, Guid courseId);

    Task<IEnumerable<CartItemDto>> GetCartAsync(Guid customerId);

    // Order operations
    Task<IEnumerable<OrderDto>> GetOrdersAsync(Guid customerId);

    Task<OrderDto?> GetOrderByIdAsync(Guid customerId, Guid orderId);

    Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync(Guid customerId, Guid orderId);

    // Payment operations
    PaymentMethodsResponseDto GetPaymentMethods();

    Task<object> ProcessPaymentAsync(Guid customerId, PaymentRequestDto request);
}
