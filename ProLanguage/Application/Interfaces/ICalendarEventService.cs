using Application.DTOs.CalendarEvent;

namespace Application.Interfaces;

public interface ICalendarEventService
{
    Task<Guid> CreateEventAsync(CreateCalendarEventDto dto);

    Task<IEnumerable<CalendarEventDto>> GetAllEventsAsync();

    Task<CalendarEventDto> GetEventByIdAsync(Guid id);

    Task UpdateEventAsync(UpdateCalendarEventDto dto);

    Task DeleteEventAsync(Guid id);
}
