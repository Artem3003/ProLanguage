using Application.Constants;
using Application.DTOs.CalendarEvent;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Tests.Fixtures;

public class CalendarEventServiceTestFixture
{
    public CalendarEventServiceTestFixture()
    {
        var cacheSettings = new CacheSettings { DefaultExpirationMinutes = 1 };
        MockCacheSettings.Setup(x => x.Value).Returns(cacheSettings);

        // Setup MemoryCache mock to handle Set extension method
        var mockCacheEntry = new Mock<ICacheEntry>();
        mockCacheEntry.SetupAllProperties();
        MockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        CalendarEventService = new CalendarEventService(
            MockUnitOfWork.Object,
            MockCalendarEventRepository.Object,
            MockMapper.Object,
            MockCacheSettings.Object,
            MockMemoryCache.Object,
            MockLogger.Object);
    }

    public Mock<IUnitOfWork> MockUnitOfWork { get; } = new();

    public Mock<ICalendarEventRepository> MockCalendarEventRepository { get; } = new();

    public Mock<IMapper> MockMapper { get; } = new();

    public Mock<IMemoryCache> MockMemoryCache { get; } = new();

    public Mock<IOptions<CacheSettings>> MockCacheSettings { get; } = new();

    public Mock<ILogger<CalendarEventService>> MockLogger { get; } = new();

    public CalendarEventService CalendarEventService { get; }

    public void ResetMocks()
    {
        MockUnitOfWork.Reset();
        MockCalendarEventRepository.Reset();
        MockMapper.Reset();
        MockMemoryCache.Reset();
        MockCacheSettings.Reset();

        // Re-setup the basic mocks that are needed for the service to function
        var cacheSettings = new CacheSettings { DefaultExpirationMinutes = 1 };
        MockCacheSettings.Setup(x => x.Value).Returns(cacheSettings);

        var mockCacheEntry = new Mock<ICacheEntry>();
        mockCacheEntry.SetupAllProperties();
        MockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);
    }

    public static CalendarEvent CreateSampleCalendarEvent()
    {
        return new CalendarEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event",
            Description = "Test Description",
            StartDateTime = DateTime.UtcNow.AddHours(1),
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Type = EventType.Lesson,
            Color = "#FF0000",
            LessonId = Guid.NewGuid(),
        };
    }

    public static CreateCalendarEventDto CreateSampleCreateCalendarEventDto()
    {
        return new CreateCalendarEventDto
        {
            Title = "New Test Event",
            Description = "New Test Description",
            StartDateTime = DateTime.UtcNow.AddHours(3),
            EndDateTime = DateTime.UtcNow.AddHours(4),
            Type = EventType.Lesson,
            Color = "#00FF00",
            LessonId = Guid.NewGuid(),
        };
    }

    public static CalendarEventDto CreateSampleCalendarEventDto()
    {
        return new CalendarEventDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Event DTO",
            Description = "Test Description DTO",
            StartDateTime = DateTime.UtcNow.AddHours(1),
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Type = EventType.Lesson,
            Color = "#FF0000",
            LessonId = Guid.NewGuid(),
        };
    }

    public static UpdateCalendarEventDto CreateSampleUpdateCalendarEventDto(Guid id)
    {
        return new UpdateCalendarEventDto
        {
            Id = id,
            Title = "Updated Test Event",
            Description = "Updated Test Description",
            StartDateTime = DateTime.UtcNow.AddHours(5),
            EndDateTime = DateTime.UtcNow.AddHours(6),
            Type = EventType.Assignment,
            Color = "#0000FF",
            LessonId = Guid.NewGuid(),
        };
    }
}