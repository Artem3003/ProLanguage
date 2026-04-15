using Application.Constants;
using Application.DTOs.Lesson;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Tests.Fixtures;

public class LessonServiceTestFixture
{
    public LessonServiceTestFixture()
    {
        // Setup cache settings
        var cacheSettings = new CacheSettings { DefaultExpirationMinutes = 1 };
        MockCacheSettings.Setup(x => x.Value).Returns(cacheSettings);

        // Setup MemoryCache mock to handle Set extension method
        var mockCacheEntry = new Mock<ICacheEntry>();
        mockCacheEntry.SetupAllProperties();
        MockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        LessonService = new LessonService(
            MockUnitOfWork.Object,
            MockLessonRepository.Object,
            MockMapper.Object,
            MockCacheSettings.Object,
            MockMemoryCache.Object,
            MockLogger.Object);
    }

    public Mock<IUnitOfWork> MockUnitOfWork { get; } = new();

    public Mock<ILessonRepository> MockLessonRepository { get; } = new();

    public Mock<IMapper> MockMapper { get; } = new();

    public Mock<IMemoryCache> MockMemoryCache { get; } = new();

    public Mock<IOptions<CacheSettings>> MockCacheSettings { get; } = new();

    public Mock<ILogger<LessonService>> MockLogger { get; } = new();

    public LessonService LessonService { get; }

    public void ResetMocks()
    {
        MockUnitOfWork.Reset();
        MockLessonRepository.Reset();
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

    /// <summary>
    /// Creates a sample Lesson entity for testing.
    /// </summary>
    public static Lesson CreateSampleLesson()
    {
        return new Lesson
        {
            Id = Guid.NewGuid(),
            Title = "Test Lesson",
            Description = "Test Description",
            DurationMinutes = 60,
            Type = Domain.Entities.Enums.LessonType.Individual,
            Status = Domain.Entities.Enums.LessonStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Creates a sample CreateLessonDto for testing.
    /// </summary>
    public static CreateLessonDto CreateSampleCreateLessonDto()
    {
        return new CreateLessonDto
        {
            Title = "New Test Lesson",
            Description = "New Test Description",
            DurationMinutes = 90,
            Type = Domain.Entities.Enums.LessonType.Group,
        };
    }

    /// <summary>
    /// Creates a sample LessonDto for testing.
    /// </summary>
    public static LessonDto CreateSampleLessonDto()
    {
        return new LessonDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Lesson DTO",
            Description = "Test Description DTO",
            DurationMinutes = 60,
            Type = Domain.Entities.Enums.LessonType.Individual,
            Status = Domain.Entities.Enums.LessonStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Creates a sample UpdateLessonDto for testing.
    /// </summary>
    public static UpdateLessonDto CreateSampleUpdateLessonDto(Guid id)
    {
        return new UpdateLessonDto
        {
            Id = id,
            Title = "Updated Lesson Title",
            Description = "Updated Description",
            DurationMinutes = 120,
        };
    }
}