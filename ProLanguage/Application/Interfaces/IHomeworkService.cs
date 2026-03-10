using Application.DTOs.Homework;

namespace Application.Interfaces;

public interface IHomeworkService
{
    Task<Guid> CreateHomeworkAsync(CreateHomeworkDto dto);

    Task<HomeworkDto> GetHomeworkByIdAsync(Guid id);

    Task<IEnumerable<HomeworkDto>> GetAllHomeworksAsync();

    Task UpdateHomeworkAsync(UpdateHomeworkDto dto);

    Task DeleteHomeworkAsync(Guid id);
}
