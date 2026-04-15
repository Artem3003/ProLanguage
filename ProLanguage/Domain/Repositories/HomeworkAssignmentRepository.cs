using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Repositories;

public class HomeworkAssignmentRepository(ApplicationDbContext context) : AbstractRepository<HomeworkAssignment>(context), IHomeworkAssignmentRepository
{
}
