using Application.DTOs.Lesson;

namespace Application.Interfaces;

public interface ILessonService
{
    Task<Guid> CreateLessonAsync(CreateLessonDto dto);

    Task<LessonDto> GetLessonByIdAsync(Guid id);

    Task<IEnumerable<LessonDto>> GetAllLessonsAsync();

    Task<int> GetTotalLessonsCountAsync();

    Task UpdateLessonAsync(UpdateLessonDto dto);

    Task DeleteLessonAsync(Guid id);
}
