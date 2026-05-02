using Application.Constants;
using Application.DTOs.Course;
using Application.Filters;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Tests.Fixtures;

public class CourseServiceTestFixture
{
    public CourseServiceTestFixture()
    {
        var cacheSettings = new CacheSettings { DefaultExpirationMinutes = 1 };
        MockCacheSettings.Setup(x => x.Value).Returns(cacheSettings);

        var mockCacheEntry = new Mock<ICacheEntry>();
        mockCacheEntry.SetupAllProperties();
        MockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        CourseService = new CourseService(
            MockUnitOfWork.Object,
            MockCourseRepository.Object,
            MockMapper.Object,
            MockCacheSettings.Object,
            MockMemoryCache.Object,
            MockDistributedCache.Object,
            MockLogger.Object,
            MockFilterPipeline.Object,
            MockPaginator.Object,
            MockCourseImageStorageService.Object);
    }

    public Mock<IUnitOfWork> MockUnitOfWork { get; } = new();

    public Mock<ICourseRepository> MockCourseRepository { get; } = new();

    public Mock<IMapper> MockMapper { get; } = new();

    public Mock<ILogger<CourseService>> MockLogger { get; } = new();

    public Mock<IOptions<CacheSettings>> MockCacheSettings { get; } = new();

    public Mock<IMemoryCache> MockMemoryCache { get; } = new();

    public Mock<IDistributedCache> MockDistributedCache { get; } = new();

    public Mock<ICourseFilterPipeline> MockFilterPipeline { get; } = new();

    public Mock<ICoursePaginator> MockPaginator { get; } = new();

    public Mock<ICourseImageStorageService> MockCourseImageStorageService { get; } = new();

    public CourseService CourseService { get; }

    public void ResetMocks()
    {
        MockUnitOfWork.Reset();
        MockCourseRepository.Reset();
        MockMapper.Reset();
        MockCacheSettings.Reset();
        MockMemoryCache.Reset();
        MockFilterPipeline.Reset();
        MockPaginator.Reset();
        MockCourseImageStorageService.Reset();

        var cacheSettings = new CacheSettings { DefaultExpirationMinutes = 1 };
        MockCacheSettings.Setup(x => x.Value).Returns(cacheSettings);

        var mockCacheEntry = new Mock<ICacheEntry>();
        mockCacheEntry.SetupAllProperties();
        MockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);
    }

    public static Course CreateSampleCourse()
    {
        return new Course
        {
            Id = Guid.NewGuid(),
            Title = "Test Course",
            Description = "Test Description",
            Price = 99.99,
            NumberOfLessons = 10,
            Lessons = [],
        };
    }

    public static CreateCourseDto CreateSampleCreateCourseDto()
    {
        return new CreateCourseDto
        {
            Title = "New Test Course",
            Description = "New Test Description",
            Price = 149.99,
            NumberOfLessons = 15,
        };
    }

    public static CourseDto CreateSampleCourseDto()
    {
        return new CourseDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Course DTO",
            Description = "Test Description DTO",
            Price = 199.99,
            NumberOfLessons = 20,
        };
    }

    public static UpdateCourseDto CreateSampleUpdateCourseDto(Guid id)
    {
        return new UpdateCourseDto
        {
            Id = id,
            Title = "Updated Test Course",
            Description = "Updated Test Description",
            Price = 249.99,
            NumberOfLessons = 25,
        };
    }
}
