using Application.Constants;
using Application.DTOs.CalendarEvent;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class CalendarEventService(IUnitOfWork unitOfWork, ICalendarEventRepository eventRepository, IMapper mapper, IOptions<CacheSettings> cacheSettings, IMemoryCache memoryCache, ILogger<CalendarEventService> logger) : ICalendarEventService
{
    private const string AllEventsCacheKey = "CalendarEvents_All";
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICalendarEventRepository _eventRepository = eventRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<CalendarEventService> _logger = logger;
    private readonly int _cacheExpirationMinutes = cacheSettings.Value.DefaultExpirationMinutes;

    public async Task<Guid> CreateEventAsync(CreateCalendarEventDto dto)
    {
        _logger.LogInformation($"Starting calendar event creation for title: {dto.Title}");

        var calendarEvent = _mapper.Map<CalendarEvent>(dto);
        if (calendarEvent is null)
        {
            _logger.LogError($"Calendar event mapping failed - unable to map request to CalendarEvent entity for title: {dto.Title}");
            throw new InvalidOperationException("Calendar event cannot be null.");
        }

        _logger.LogDebug($"Mapped calendar event DTO to entity for title: {dto.Title}");

        await _eventRepository.AddAsync(calendarEvent);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache immediately after create
        _memoryCache.Remove(AllEventsCacheKey);
        _logger.LogDebug($"Cleared calendar events cache after creation");

        _logger.LogInformation($"Successfully created calendar event with ID: {calendarEvent.Id}, Title: {calendarEvent.Title}");

        return calendarEvent.Id;
    }

    public async Task<IEnumerable<CalendarEventDto>> GetAllEventsAsync()
    {
        _logger.LogInformation("Retrieving all calendar events");

        // Try to get from cache
        if (_memoryCache.TryGetValue(AllEventsCacheKey, out IEnumerable<CalendarEventDto>? cachedEvents) && cachedEvents is not null)
        {
            _logger.LogDebug("Retrieved all calendar events from cache");
            return cachedEvents;
        }

        // Get from database
        var events = await _eventRepository.GetAllAsync();
        var eventDtos = _mapper.Map<IEnumerable<CalendarEventDto>>(events);

        // Store in cache
        _memoryCache.Set(AllEventsCacheKey, eventDtos, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _logger.LogDebug("Cached all calendar events");

        _logger.LogInformation($"Successfully retrieved {eventDtos.Count()} calendar events");

        return eventDtos;
    }

    public async Task<CalendarEventDto> GetEventByIdAsync(Guid id)
    {
        _logger.LogInformation($"Retrieving calendar event by ID: {id}");

        var calendarEvent = await _eventRepository.GetByIdAsync(id);
        if (calendarEvent is null)
        {
            _logger.LogError($"Calendar event with ID {id} not found.");
            throw new KeyNotFoundException($"Calendar event with ID {id} not found.");
        }

        _logger.LogDebug($"Calendar event retrieved from repository for ID: {id}");

        var eventDto = _mapper.Map<CalendarEventDto>(calendarEvent);

        _logger.LogInformation($"Successfully retrieved calendar event: {id}");

        return eventDto;
    }

    public async Task UpdateEventAsync(UpdateCalendarEventDto dto)
    {
        _logger.LogInformation($"Starting calendar event update for ID: {dto.Id}");

        var calendarEvent = await _eventRepository.GetByIdAsync(dto.Id);
        if (calendarEvent is null)
        {
            _logger.LogError($"Calendar event with ID {dto.Id} not found.");
            throw new KeyNotFoundException($"Calendar event with ID {dto.Id} not found.");
        }

        _logger.LogDebug($"Found existing calendar event: {calendarEvent.Id}, Title: {calendarEvent.Title}");

        _mapper.Map(dto, calendarEvent);
        _eventRepository.Update(calendarEvent);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache immediately after update
        _memoryCache.Remove(AllEventsCacheKey);
        _logger.LogDebug($"Cleared calendar events cache after update");

        _logger.LogInformation($"Successfully updated calendar event: {calendarEvent.Id}, Title: {calendarEvent.Title}");
    }

    public async Task DeleteEventAsync(Guid id)
    {
        _logger.LogInformation($"Starting calendar event deletion for ID: {id}");

        var calendarEvent = await _eventRepository.GetByIdAsync(id);
        if (calendarEvent is null)
        {
            _logger.LogError($"Calendar event with ID {id} not found.");
            throw new KeyNotFoundException($"Calendar event with ID {id} not found.");
        }

        _eventRepository.Delete(calendarEvent);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache immediately after delete
        _memoryCache.Remove(AllEventsCacheKey);
        _logger.LogDebug($"Cleared calendar events cache after deletion");

        _logger.LogInformation($"Successfully deleted calendar event: {calendarEvent.Id}");
    }
}
