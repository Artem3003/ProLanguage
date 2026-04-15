using Domain.Entities;
using Domain.Entities.Enums;

namespace Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOpenOrderByCustomerIdAsync(Guid customerId);

    Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId);

    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);

    Task<IEnumerable<Order>> GetPaidAndCancelledOrdersByCustomerIdAsync(Guid customerId);

    Task<Order?> GetOrderWithCoursesAsync(Guid orderId);
}
