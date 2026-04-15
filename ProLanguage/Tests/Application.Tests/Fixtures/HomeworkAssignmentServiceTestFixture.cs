using Application.Constants;
using Application.DTOs.HomeworkAssignment;
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

public class HomeworkAssignmentServiceTestFixture
{
    public HomeworkAssignmentServiceTestFixture()
    {
        var cacheSettings = new CacheSettings { DefaultExpirationMinutes = 1 };
        MockCacheSettings.Setup(x => x.Value).Returns(cacheSettings);

        // Setup MemoryCache mock to handle Set extension method
        var mockCacheEntry = new Mock<ICacheEntry>();
        mockCacheEntry.SetupAllProperties();
        MockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        HomeworkAssignmentService = new HomeworkAssignmentService(
            MockUnitOfWork.Object,
            MockHomeworkAssignmentRepository.Object,
            MockMapper.Object,
            MockCacheSettings.Object,
            MockMemoryCache.Object,
            MockLogger.Object);
    }

    public Mock<IUnitOfWork> MockUnitOfWork { get; } = new();

    public Mock<IHomeworkAssignmentRepository> MockHomeworkAssignmentRepository { get; } = new();

    public Mock<IMapper> MockMapper { get; } = new();

    public Mock<IMemoryCache> MockMemoryCache { get; } = new();

    public Mock<IOptions<CacheSettings>> MockCacheSettings { get; } = new();

    public Mock<ILogger<HomeworkAssignmentService>> MockLogger { get; } = new();

    public HomeworkAssignmentService HomeworkAssignmentService { get; }

    public void ResetMocks()
    {
        MockUnitOfWork.Reset();
        MockHomeworkAssignmentRepository.Reset();
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

    public static HomeworkAssignment CreateSampleHomeworkAssignment()
    {
        return new HomeworkAssignment
        {
            Id = Guid.NewGuid(),
            HomeworkId = Guid.NewGuid(),
            SubmissionText = "Test submission",
            AttachmentUrl = "https://example.com/file.pdf",
            SubmittedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Submitted,
            Grade = 85,
            TeacherFeedback = "Good work",
            GradedAt = DateTime.UtcNow.AddHours(1),
        };
    }

    public static CreateHomeworkAssignmentDto CreateSampleCreateHomeworkAssignmentDto()
    {
        return new CreateHomeworkAssignmentDto
        {
            HomeworkId = Guid.NewGuid(),
        };
    }

    public static HomeworkAssignmentDto CreateSampleHomeworkAssignmentDto()
    {
        return new HomeworkAssignmentDto
        {
            Id = Guid.NewGuid(),
            HomeworkId = Guid.NewGuid(),
            SubmissionText = "Test submission DTO",
            AttachmentUrl = "https://example.com/dto-file.pdf",
            SubmittedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Submitted,
            Grade = 90,
            TeacherFeedback = "Great work!",
            GradedAt = DateTime.UtcNow.AddHours(2),
        };
    }

    public static SubmitHomeworkAssignmentDto CreateSampleSubmitHomeworkAssignmentDto()
    {
        return new SubmitHomeworkAssignmentDto
        {
            SubmissionText = "Here is my submission for the homework assignment.",
            AttachmentUrl = "https://example.com/submission.pdf",
        };
    }

    public static GradeHomeworkAssignmentDto CreateSampleGradeHomeworkAssignmentDto()
    {
        return new GradeHomeworkAssignmentDto
        {
            Grade = 95,
            TeacherFeedback = "Excellent work! Well done on this assignment.",
        };
    }
}