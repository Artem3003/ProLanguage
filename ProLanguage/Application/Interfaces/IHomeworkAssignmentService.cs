using Application.DTOs.HomeworkAssignment;

namespace Application.Interfaces;

public interface IHomeworkAssignmentService
{
    Task<Guid> CreateAssignmentAsync(CreateHomeworkAssignmentDto dto);

    Task<HomeworkAssignmentDto> GetAssignmentByIdAsync(Guid id);

    Task<IEnumerable<HomeworkAssignmentDto>> GetAllAssignmentsAsync();

    Task SubmitAssignmentAsync(Guid id, SubmitHomeworkAssignmentDto dto);

    Task GradeAssignmentAsync(Guid id, GradeHomeworkAssignmentDto dto);

    Task DeleteAssignmentAsync(Guid id);
}
