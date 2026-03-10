using Domain.Entities.Enums;

namespace Application.DTOs.HomeworkAssignment;

public class HomeworkAssignmentDto
{
    public Guid Id { get; set; }

    public Guid HomeworkId { get; set; }

    public string? SubmissionText { get; set; }

    public string? AttachmentUrl { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public AssignmentStatus Status { get; set; }

    public int? Grade { get; set; }

    public string? TeacherFeedback { get; set; }

    public DateTime? GradedAt { get; set; }
}
