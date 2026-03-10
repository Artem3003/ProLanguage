using Domain.Entities.Enums;

namespace Application.DTOs.HomeworkAssignment;

public class CreateHomeworkAssignmentDto
{
    public Guid HomeworkId { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
}
