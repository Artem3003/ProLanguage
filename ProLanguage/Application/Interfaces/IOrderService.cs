using Application.DTOs.Order;

namespace Application.Interfaces;

public interface IOrderService
{
    // Cart operations
    Task AddToCartAsync(Guid courseId);

    Task RemoveFromCartAsync(Guid courseId);

    Task<IEnumerable<CartItemDto>> GetCartAsync();

    // Order operations
    Task<IEnumerable<OrderDto>> GetOrdersAsync();

    Task<OrderDto?> GetOrderByIdAsync(Guid orderId);

    Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync(Guid orderId);

    // Payment operations
    PaymentMethodsResponseDto GetPaymentMethods();

    Task<object> ProcessPaymentAsync(PaymentRequestDto request);
}
