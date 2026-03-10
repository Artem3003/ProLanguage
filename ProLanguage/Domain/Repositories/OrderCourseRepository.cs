using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class OrderCourseRepository(ApplicationDbContext context) : AbstractRepository<OrderCourse>(context), IOrderCourseRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<OrderCourse?> GetByOrderAndCourseAsync(Guid orderId, Guid courseId)
    {
        return await _context.OrderCourses
            .FirstOrDefaultAsync(oc => oc.OrderId == orderId && oc.CourseId == courseId);
    }

    public async Task<IEnumerable<OrderCourse>> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.OrderCourses
            .Include(oc => oc.Course)
            .Where(oc => oc.OrderId == orderId)
            .ToListAsync();
    }

    public async Task DeleteByOrderAndCourseAsync(Guid orderId, Guid courseId)
    {
        var orderCourse = await GetByOrderAndCourseAsync(orderId, courseId);
        if (orderCourse != null)
        {
            Delete(orderCourse);
        }
    }
}
