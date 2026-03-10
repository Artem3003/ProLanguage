using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrderCourseRepository : IRepository<OrderCourse>
{
    Task<OrderCourse?> GetByOrderAndCourseAsync(Guid orderId, Guid courseId);

    Task<IEnumerable<OrderCourse>> GetByOrderIdAsync(Guid orderId);

    Task DeleteByOrderAndCourseAsync(Guid orderId, Guid courseId);
}
