using Domain.Entities.Common;
using Domain.Entities.Enums;

namespace Domain.Entities;

public class Order : BaseEntity<Guid>
{
    public DateTime? Date { get; set; }

    public Guid CustomerId { get; set; }

    public OrderStatus Status { get; set; }

    public List<OrderCourse> OrderCourses { get; set; } = [];
}
