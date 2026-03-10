using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Repositories;

public class HomeworkRepository(ApplicationDbContext context) : AbstractRepository<Homework>(context), IHomeworkRepository
{
}
