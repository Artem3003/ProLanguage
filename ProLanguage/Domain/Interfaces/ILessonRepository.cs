using Domain.Entities;

namespace Domain.Interfaces;

public interface ILessonRepository : IRepository<Lesson>
{
    Task<int> CountLessonsAsync();
}
