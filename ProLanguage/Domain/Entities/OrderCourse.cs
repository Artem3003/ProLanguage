using Domain.Entities.Common;

namespace Domain.Entities;

public class OrderCourse : BaseEntity<Guid>
{
    public Guid OrderId { get; set; }

    public Guid CourseId { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public int Discount { get; set; }

    public Order Order { get; set; }

    public Course Course { get; set; }
}
