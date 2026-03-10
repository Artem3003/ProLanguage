using Domain.Data;
using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Repositories;

public class CalendarEventRepository(ApplicationDbContext context) : AbstractRepository<CalendarEvent>(context), ICalendarEventRepository
{
}
