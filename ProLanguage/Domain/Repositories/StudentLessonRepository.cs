using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Repositories;

public class StudentLessonRepository(ApplicationDbContext context) : AbstractRepository<StudentLesson>(context), IStudentLessonRepository
{
}
