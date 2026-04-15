using Application.Constants;
using Application.DTOs.Homework;
using Application.Tests.Fixtures;
using Domain.Entities;
using Moq;

namespace Application.Tests.ServicesTests;

public class HomeworkServiceTests(HomeworkServiceTestFixture fixture) : IClassFixture<HomeworkServiceTestFixture>
{
    private readonly HomeworkServiceTestFixture _fixture = fixture;

    [Fact]
    public async Task CreateHomeworkAsync_ShouldCreateHomeworkAndCacheResult_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var createDto = HomeworkServiceTestFixture.CreateSampleCreateHomeworkDto();
        var homework = HomeworkServiceTestFixture.CreateSampleHomework();
        var homeworkDto = HomeworkServiceTestFixture.CreateSampleHomeworkDto();

        _fixture.MockMapper.Setup(m => m.Map<Homework>(createDto)).Returns(homework);
        _fixture.MockMapper.Setup(m => m.Map<HomeworkDto>(homework)).Returns(homeworkDto);
        _fixture.MockHomeworkRepository.Setup(r => r.AddAsync(It.IsAny<Homework>())).Returns(Task.FromResult(homework));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        var result = await _fixture.HomeworkService.CreateHomeworkAsync(createDto);

        // Assert
        Assert.Equal(homework.Id, result);
        _fixture.MockHomeworkRepository.Verify(r => r.AddAsync(homework), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.CreateEntry(CacheKeys.Homework), Times.Once);
    }

    [Fact]
    public async Task GetHomeworkByIdAsync_ShouldReturnCachedHomework_WhenHomeworkExistsInCache()
    {
        // Arrange
        _fixture.ResetMocks();
        var homeworkId = Guid.NewGuid();
        var cachedHomework = HomeworkServiceTestFixture.CreateSampleHomeworkDto();
        object cachedValue = cachedHomework;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.Homework, out cachedValue)).Returns(true);

        // Act
        var result = await _fixture.HomeworkService.GetHomeworkByIdAsync(homeworkId);

        // Assert
        Assert.Equal(cachedHomework, result);
        _fixture.MockHomeworkRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetHomeworkByIdAsync_ShouldReturnHomeworkFromRepository_WhenNotInCache()
    {
        // Arrange
        var homeworkId = Guid.NewGuid();
        var homework = HomeworkServiceTestFixture.CreateSampleHomework();
        homework.Id = homeworkId;
        var homeworkDto = HomeworkServiceTestFixture.CreateSampleHomeworkDto();
        object cachedValue = null!;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.Homework, out cachedValue)).Returns(false);
        _fixture.MockHomeworkRepository.Setup(r => r.GetByIdAsync(homeworkId)).ReturnsAsync(homework);
        _fixture.MockMapper.Setup(m => m.Map<HomeworkDto>(homework)).Returns(homeworkDto);

        // Act
        var result = await _fixture.HomeworkService.GetHomeworkByIdAsync(homeworkId);

        // Assert
        Assert.Equal(homeworkDto, result);
        _fixture.MockHomeworkRepository.Verify(r => r.GetByIdAsync(homeworkId), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.CreateEntry(CacheKeys.Homework), Times.Once);
    }

    [Fact]
    public async Task GetHomeworkByIdAsync_ShouldThrowKeyNotFoundException_WhenHomeworkNotFound()
    {
        // Arrange
        var homeworkId = Guid.NewGuid();
        object cachedValue = null!;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.Homework, out cachedValue)).Returns(false);
        _fixture.MockHomeworkRepository.Setup(r => r.GetByIdAsync(homeworkId)).ReturnsAsync((Homework?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.HomeworkService.GetHomeworkByIdAsync(homeworkId));
        Assert.Contains(homeworkId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetAllHomeworksAsync_ShouldReturnAllHomeworks_WhenCalled()
    {
        // Arrange
        var homeworks = new List<Homework>
        {
            HomeworkServiceTestFixture.CreateSampleHomework(),
            HomeworkServiceTestFixture.CreateSampleHomework(),
        };
        var homeworkDtos = new List<HomeworkDto>
        {
            HomeworkServiceTestFixture.CreateSampleHomeworkDto(),
            HomeworkServiceTestFixture.CreateSampleHomeworkDto(),
        };

        _fixture.MockHomeworkRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(homeworks);
        _fixture.MockMapper.Setup(m => m.Map<IEnumerable<HomeworkDto>>(homeworks)).Returns(homeworkDtos);

        // Act
        var result = await _fixture.HomeworkService.GetAllHomeworksAsync();

        // Assert
        Assert.Equal(homeworkDtos, result);
        _fixture.MockHomeworkRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteHomeworkAsync_ShouldDeleteHomeworkAndInvalidateCache_WhenHomeworkExists()
    {
        // Arrange
        var homeworkId = Guid.NewGuid();
        var existingHomework = HomeworkServiceTestFixture.CreateSampleHomework();
        existingHomework.Id = homeworkId;

        _fixture.MockHomeworkRepository.Setup(r => r.GetByIdAsync(homeworkId)).ReturnsAsync(existingHomework);
        _fixture.MockHomeworkRepository.Setup(r => r.Delete(existingHomework)).Verifiable();
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        await _fixture.HomeworkService.DeleteHomeworkAsync(homeworkId);

        // Assert
        _fixture.MockHomeworkRepository.Verify(r => r.GetByIdAsync(homeworkId), Times.Once);
        _fixture.MockHomeworkRepository.Verify(r => r.Delete(existingHomework), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.Homework), Times.Once);
    }
}