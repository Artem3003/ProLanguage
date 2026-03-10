using Application.Constants;
using Application.DTOs.Homework;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Tests.Fixtures;

public class HomeworkServiceTestFixture
{
    public HomeworkServiceTestFixture()
    {
        var cacheSettings = new CacheSettings { DefaultExpirationMinutes = 1 };
        MockCacheSettings.Setup(x => x.Value).Returns(cacheSettings);

        // Setup MemoryCache mock to handle Set extension method
        var mockCacheEntry = new Mock<ICacheEntry>();
        mockCacheEntry.SetupAllProperties();
        MockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        HomeworkService = new HomeworkService(
            MockUnitOfWork.Object,
            MockHomeworkRepository.Object,
            MockMapper.Object,
            MockCacheSettings.Object,
            MockMemoryCache.Object,
            MockLogger.Object);
    }

    public Mock<IUnitOfWork> MockUnitOfWork { get; } = new();

    public Mock<IHomeworkRepository> MockHomeworkRepository { get; } = new();

    public Mock<IMapper> MockMapper { get; } = new();

    public Mock<IMemoryCache> MockMemoryCache { get; } = new();

    public Mock<IOptions<CacheSettings>> MockCacheSettings { get; } = new();

    public Mock<ILogger<HomeworkService>> MockLogger { get; } = new();

    public HomeworkService HomeworkService { get; }

    public void ResetMocks()
    {
        MockUnitOfWork.Reset();
        MockHomeworkRepository.Reset();
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

    public static Homework CreateSampleHomework()
    {
        return new Homework
        {
            Id = Guid.NewGuid(),
            Title = "Test Homework",
            Description = "Test Description",
            Instructions = "Test Instructions",
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            LessonId = Guid.NewGuid(),
        };
    }

    public static CreateHomeworkDto CreateSampleCreateHomeworkDto()
    {
        return new CreateHomeworkDto
        {
            Title = "New Homework",
            Description = "New Description",
            Instructions = "New Instructions",
            DueDate = DateTime.UtcNow.AddDays(5),
            LessonId = Guid.NewGuid(),
        };
    }

    public static HomeworkDto CreateSampleHomeworkDto()
    {
        return new HomeworkDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Homework DTO",
            Description = "Test Description DTO",
            Instructions = "Test Instructions DTO",
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            LessonId = Guid.NewGuid(),
        };
    }
}