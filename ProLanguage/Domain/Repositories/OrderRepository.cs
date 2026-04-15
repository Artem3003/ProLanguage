using Domain.Data;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class OrderRepository(ApplicationDbContext context) : AbstractRepository<Order>(context), IOrderRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Order?> GetOpenOrderByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderCourses)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.Status == OrderStatus.Open);
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderCourses)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.OrderCourses)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetPaidAndCancelledOrdersByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderCourses)
            .Where(o => o.CustomerId == customerId && (o.Status == OrderStatus.Paid || o.Status == OrderStatus.Cancelled))
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithCoursesAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderCourses)
                .ThenInclude(oc => oc.Course)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public override async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.OrderCourses)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
