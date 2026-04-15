using Application.Constants;
using Application.DTOs.Lesson;
using Application.Tests.Fixtures;
using Domain.Entities;
using Moq;

namespace Application.Tests.ServicesTests;

public class LessonServiceTests(LessonServiceTestFixture fixture) : IClassFixture<LessonServiceTestFixture>
{
    private readonly LessonServiceTestFixture _fixture = fixture;

    [Fact]
    public async Task CreateLessonAsync_ShouldCreateLessonAndInvalidateCache_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var createDto = LessonServiceTestFixture.CreateSampleCreateLessonDto();
        var lesson = LessonServiceTestFixture.CreateSampleLesson();
        var lessonDto = LessonServiceTestFixture.CreateSampleLessonDto();

        _fixture.MockMapper.Setup(m => m.Map<Lesson>(createDto)).Returns(lesson);
        _fixture.MockMapper.Setup(m => m.Map<LessonDto>(lesson)).Returns(lessonDto);
        _fixture.MockLessonRepository.Setup(r => r.AddAsync(It.IsAny<Lesson>())).Returns(Task.FromResult(lesson));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        var result = await _fixture.LessonService.CreateLessonAsync(createDto);

        // Assert
        Assert.Equal(lesson.Id, result);
        _fixture.MockLessonRepository.Verify(r => r.AddAsync(lesson), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.CreateEntry(CacheKeys.Lessons), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.TotalLessonsCount), Times.Once);
    }

    [Fact]
    public async Task GetLessonByIdAsync_ShouldReturnCachedLesson_WhenLessonExistsInCache()
    {
        // Arrange
        _fixture.ResetMocks();
        var lessonId = Guid.NewGuid();
        var cachedLesson = LessonServiceTestFixture.CreateSampleLessonDto();
        object cachedValue = cachedLesson;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.Lessons, out cachedValue)).Returns(true);

        // Act
        var result = await _fixture.LessonService.GetLessonByIdAsync(lessonId);

        // Assert
        Assert.Equal(cachedLesson, result);
        _fixture.MockLessonRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetLessonByIdAsync_ShouldReturnLessonFromRepository_WhenNotInCache()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var lesson = LessonServiceTestFixture.CreateSampleLesson();
        lesson.Id = lessonId;
        var lessonDto = LessonServiceTestFixture.CreateSampleLessonDto();
        object cachedValue = null!;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.Lessons, out cachedValue)).Returns(false);
        _fixture.MockLessonRepository.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync(lesson);
        _fixture.MockMapper.Setup(m => m.Map<LessonDto>(lesson)).Returns(lessonDto);

        // Act
        var result = await _fixture.LessonService.GetLessonByIdAsync(lessonId);

        // Assert
        Assert.Equal(lessonDto, result);
        _fixture.MockLessonRepository.Verify(r => r.GetByIdAsync(lessonId), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.CreateEntry(CacheKeys.Lessons), Times.Once);
    }

    [Fact]
    public async Task GetLessonByIdAsync_ShouldThrowKeyNotFoundException_WhenLessonNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        object cachedValue = null!;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.Lessons, out cachedValue)).Returns(false);
        _fixture.MockLessonRepository.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync((Lesson?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.LessonService.GetLessonByIdAsync(lessonId));
        Assert.Contains(lessonId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetAllLessonsAsync_ShouldReturnAllLessons_WhenCalled()
    {
        // Arrange
        var lessons = new List<Lesson>
        {
            LessonServiceTestFixture.CreateSampleLesson(),
            LessonServiceTestFixture.CreateSampleLesson(),
        };
        var lessonDtos = new List<LessonDto>
        {
            LessonServiceTestFixture.CreateSampleLessonDto(),
            LessonServiceTestFixture.CreateSampleLessonDto(),
        };

        _fixture.MockLessonRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(lessons);
        _fixture.MockMapper.Setup(m => m.Map<IEnumerable<LessonDto>>(lessons)).Returns(lessonDtos);

        // Act
        var result = await _fixture.LessonService.GetAllLessonsAsync();

        // Assert
        Assert.Equal(lessonDtos, result);
        _fixture.MockLessonRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTotalLessonsCountAsync_ShouldReturnCachedCount_WhenCountExistsInCache()
    {
        // Arrange
        const int cachedCount = 42;
        object cachedValue = cachedCount;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.TotalLessonsCount, out cachedValue)).Returns(true);

        // Act
        var result = await _fixture.LessonService.GetTotalLessonsCountAsync();

        // Assert
        Assert.Equal(cachedCount, result);
        _fixture.MockLessonRepository.Verify(r => r.CountLessonsAsync(), Times.Never);
    }

    [Fact]
    public async Task GetTotalLessonsCountAsync_ShouldReturnCountFromRepository_WhenNotInCache()
    {
        // Arrange
        const int repositoryCount = 25;
        object cachedValue = null!;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.TotalLessonsCount, out cachedValue)).Returns(false);
        _fixture.MockLessonRepository.Setup(r => r.CountLessonsAsync()).ReturnsAsync(repositoryCount);

        // Act
        var result = await _fixture.LessonService.GetTotalLessonsCountAsync();

        // Assert
        Assert.Equal(repositoryCount, result);
        _fixture.MockLessonRepository.Verify(r => r.CountLessonsAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.CreateEntry(CacheKeys.TotalLessonsCount), Times.Once);
    }

    [Fact]
    public async Task UpdateLessonAsync_ShouldUpdateLessonAndInvalidateCache_WhenValidDtoProvided()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var updateDto = LessonServiceTestFixture.CreateSampleUpdateLessonDto(lessonId);
        var existingLesson = LessonServiceTestFixture.CreateSampleLesson();
        existingLesson.Id = lessonId;

        _fixture.MockLessonRepository.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync(existingLesson);
        _fixture.MockMapper.Setup(m => m.Map(updateDto, existingLesson)).Verifiable();
        _fixture.MockLessonRepository.Setup(r => r.Update(existingLesson)).Verifiable();
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        await _fixture.LessonService.UpdateLessonAsync(updateDto);

        // Assert
        _fixture.MockLessonRepository.Verify(r => r.GetByIdAsync(lessonId), Times.Once);
        _fixture.MockMapper.Verify(m => m.Map(updateDto, existingLesson), Times.Once);
        _fixture.MockLessonRepository.Verify(r => r.Update(existingLesson), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.Lessons), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.TotalLessonsCount), Times.Once);
    }

    [Fact]
    public async Task UpdateLessonAsync_ShouldThrowKeyNotFoundException_WhenLessonNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var updateDto = LessonServiceTestFixture.CreateSampleUpdateLessonDto(lessonId);

        _fixture.MockLessonRepository.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync((Lesson?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.LessonService.UpdateLessonAsync(updateDto));
        Assert.Contains(lessonId.ToString(), exception.Message);
    }

    [Fact]
    public async Task DeleteLessonAsync_ShouldDeleteLessonAndInvalidateCache_WhenLessonExists()
    {
        // Arrange
        _fixture.ResetMocks();
        var lessonId = Guid.NewGuid();
        var existingLesson = LessonServiceTestFixture.CreateSampleLesson();
        existingLesson.Id = lessonId;

        _fixture.MockLessonRepository.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync(existingLesson);
        _fixture.MockLessonRepository.Setup(r => r.Delete(existingLesson)).Verifiable();
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        await _fixture.LessonService.DeleteLessonAsync(lessonId);

        // Assert
        _fixture.MockLessonRepository.Verify(r => r.GetByIdAsync(lessonId), Times.Once);
        _fixture.MockLessonRepository.Verify(r => r.Delete(existingLesson), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.Lessons), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.TotalLessonsCount), Times.Once);
    }

    [Fact]
    public async Task DeleteLessonAsync_ShouldThrowKeyNotFoundException_WhenLessonNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();

        _fixture.MockLessonRepository.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync((Lesson?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.LessonService.DeleteLessonAsync(lessonId));
        Assert.Contains(lessonId.ToString(), exception.Message);
    }
}