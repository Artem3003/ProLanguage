using Application.DTOs.CalendarEvent;
using Application.Tests.Fixtures;
using Domain.Entities;
using Moq;

namespace Application.Tests.ServicesTests;

public class CalendarEventServiceTests(CalendarEventServiceTestFixture fixture) : IClassFixture<CalendarEventServiceTestFixture>
{
    private readonly CalendarEventServiceTestFixture _fixture = fixture;

    [Fact]
    public async Task CreateEventAsync_ShouldCreateEventAndCacheResult_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var createDto = CalendarEventServiceTestFixture.CreateSampleCreateCalendarEventDto();
        var calendarEvent = CalendarEventServiceTestFixture.CreateSampleCalendarEvent();

        _fixture.MockMapper.Setup(m => m.Map<CalendarEvent>(createDto)).Returns(calendarEvent);
        _fixture.MockCalendarEventRepository.Setup(r => r.AddAsync(It.IsAny<CalendarEvent>())).Returns(Task.FromResult(calendarEvent));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        var result = await _fixture.CalendarEventService.CreateEventAsync(createDto);

        // Assert
        Assert.Equal(calendarEvent.Id, result);
        _fixture.MockCalendarEventRepository.Verify(r => r.AddAsync(calendarEvent), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove("CalendarEvents_All"), Times.Once);
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnCachedEvent_WhenEventExistsInCache()
    {
        // Arrange
        _fixture.ResetMocks();
        var eventId = Guid.NewGuid();
        var calendarEvent = CalendarEventServiceTestFixture.CreateSampleCalendarEvent();
        calendarEvent.Id = eventId;
        var eventDto = CalendarEventServiceTestFixture.CreateSampleCalendarEventDto();

        _fixture.MockCalendarEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(calendarEvent);
        _fixture.MockMapper.Setup(m => m.Map<CalendarEventDto>(calendarEvent)).Returns(eventDto);

        // Act
        var result = await _fixture.CalendarEventService.GetEventByIdAsync(eventId);

        // Assert
        Assert.Equal(eventDto, result);
        _fixture.MockCalendarEventRepository.Verify(r => r.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnEventFromRepository_WhenNotInCache()
    {
        // Arrange
        _fixture.ResetMocks();
        var eventId = Guid.NewGuid();
        var calendarEvent = CalendarEventServiceTestFixture.CreateSampleCalendarEvent();
        calendarEvent.Id = eventId;
        var eventDto = CalendarEventServiceTestFixture.CreateSampleCalendarEventDto();

        _fixture.MockCalendarEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(calendarEvent);
        _fixture.MockMapper.Setup(m => m.Map<CalendarEventDto>(calendarEvent)).Returns(eventDto);

        // Act
        var result = await _fixture.CalendarEventService.GetEventByIdAsync(eventId);

        // Assert
        Assert.Equal(eventDto, result);
        _fixture.MockCalendarEventRepository.Verify(r => r.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldThrowKeyNotFoundException_WhenEventNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var eventId = Guid.NewGuid();

        _fixture.MockCalendarEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync((CalendarEvent?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.CalendarEventService.GetEventByIdAsync(eventId));
        Assert.Contains(eventId.ToString(), exception.Message);
    }

    [Fact]
    public async Task UpdateEventAsync_ShouldUpdateEventAndInvalidateCache_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var eventId = Guid.NewGuid();
        var existingEvent = CalendarEventServiceTestFixture.CreateSampleCalendarEvent();
        existingEvent.Id = eventId;
        var updateDto = CalendarEventServiceTestFixture.CreateSampleUpdateCalendarEventDto(eventId);

        _fixture.MockCalendarEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
        _fixture.MockMapper.Setup(m => m.Map(updateDto, existingEvent));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _fixture.CalendarEventService.UpdateEventAsync(updateDto);

        // Assert
        _fixture.MockCalendarEventRepository.Verify(r => r.GetByIdAsync(eventId), Times.Once);
        _fixture.MockMapper.Verify(m => m.Map(updateDto, existingEvent), Times.Once);
        _fixture.MockCalendarEventRepository.Verify(r => r.Update(existingEvent), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove("CalendarEvents_All"), Times.Once);
    }

    [Fact]
    public async Task UpdateEventAsync_ShouldThrowKeyNotFoundException_WhenEventNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var eventId = Guid.NewGuid();
        var updateDto = CalendarEventServiceTestFixture.CreateSampleUpdateCalendarEventDto(eventId);

        _fixture.MockCalendarEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync((CalendarEvent?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.CalendarEventService.UpdateEventAsync(updateDto));
        Assert.Contains(eventId.ToString(), exception.Message);
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldDeleteEventAndInvalidateCache_WhenEventExists()
    {
        // Arrange
        _fixture.ResetMocks();
        var eventId = Guid.NewGuid();
        var existingEvent = CalendarEventServiceTestFixture.CreateSampleCalendarEvent();
        existingEvent.Id = eventId;

        _fixture.MockCalendarEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
        _fixture.MockCalendarEventRepository.Setup(r => r.Delete(existingEvent)).Verifiable();
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        await _fixture.CalendarEventService.DeleteEventAsync(eventId);

        // Assert
        _fixture.MockCalendarEventRepository.Verify(r => r.GetByIdAsync(eventId), Times.Once);
        _fixture.MockCalendarEventRepository.Verify(r => r.Delete(existingEvent), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove("CalendarEvents_All"), Times.Once);
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldThrowKeyNotFoundException_WhenEventNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var eventId = Guid.NewGuid();

        _fixture.MockCalendarEventRepository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync((CalendarEvent?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.CalendarEventService.DeleteEventAsync(eventId));
        Assert.Contains(eventId.ToString(), exception.Message);
    }
}