using Application.DTOs.CalendarEvent;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Calendar Events")]
[Authorize]
public class CalendarController(ICalendarEventService calendarEventService) : ControllerBase
{
    private readonly ICalendarEventService _calendarEventService = calendarEventService;

    [HttpGet]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetAllEvents()
    {
        var events = await _calendarEventService.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpPost]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult<Guid>> CreateEvent([FromBody] CreateCalendarEventDto request)
    {
        var eventId = await _calendarEventService.CreateEventAsync(request);
        return CreatedAtAction(nameof(GetEventById), new { id = eventId }, eventId);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult<CalendarEventDto>> GetEventById(Guid id)
    {
        var calendarEvent = await _calendarEventService.GetEventByIdAsync(id);
        return Ok(calendarEvent);
    }

    [HttpPut]
    [Authorize(Policy = "TeacherAccess")]
    public async Task<ActionResult> UpdateEvent([FromBody] UpdateCalendarEventDto request)
    {
        await _calendarEventService.UpdateEventAsync(request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "TeacherAccess")]
    public async Task<ActionResult> DeleteEvent(Guid id)
    {
        await _calendarEventService.DeleteEventAsync(id);
        return NoContent();
    }
}