using Application.Constants;
using Application.DTOs.HomeworkAssignment;
using Application.Tests.Fixtures;
using Domain.Entities;
using Moq;

namespace Application.Tests.ServicesTests;

public class HomeworkAssignmentServiceTests(HomeworkAssignmentServiceTestFixture fixture) : IClassFixture<HomeworkAssignmentServiceTestFixture>
{
    private readonly HomeworkAssignmentServiceTestFixture _fixture = fixture;

    [Fact]
    public async Task CreateAssignmentAsync_ShouldCreateAssignmentAndCacheResult_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var createDto = HomeworkAssignmentServiceTestFixture.CreateSampleCreateHomeworkAssignmentDto();
        var assignment = HomeworkAssignmentServiceTestFixture.CreateSampleHomeworkAssignment();
        var assignmentDto = new HomeworkAssignmentDto
        {
            Id = assignment.Id,
            HomeworkId = assignment.HomeworkId,
            SubmissionText = assignment.SubmissionText,
            Status = assignment.Status,
        };

        _fixture.MockMapper.Setup(m => m.Map<HomeworkAssignment>(createDto)).Returns(assignment);
        _fixture.MockMapper.Setup(m => m.Map<HomeworkAssignmentDto>(assignment)).Returns(assignmentDto);
        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.AddAsync(It.IsAny<HomeworkAssignment>())).Returns(Task.FromResult(assignment));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        var result = await _fixture.HomeworkAssignmentService.CreateAssignmentAsync(createDto);

        // Assert
        Assert.Equal(assignment.Id, result);
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.AddAsync(assignment), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.CreateEntry(CacheKeys.HomeworkAssignments), Times.Once);
    }

    [Fact]
    public async Task GetAssignmentByIdAsync_ShouldReturnCachedAssignment_WhenAssignmentExistsInCache()
    {
        // Arrange
        _fixture.ResetMocks();
        var assignmentId = Guid.NewGuid();
        var cachedAssignment = new HomeworkAssignmentDto
        {
            Id = assignmentId,
            HomeworkId = Guid.NewGuid(),
            SubmissionText = "Cached submission",
        };
        object cachedValue = cachedAssignment;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.HomeworkAssignments, out cachedValue)).Returns(true);

        // Act
        var result = await _fixture.HomeworkAssignmentService.GetAssignmentByIdAsync(assignmentId);

        // Assert
        Assert.Equal(cachedAssignment, result);
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetAssignmentByIdAsync_ShouldThrowKeyNotFoundException_WhenAssignmentNotFound()
    {
        // Arrange
        var assignmentId = Guid.NewGuid();
        object cachedValue = null!;

        _fixture.MockMemoryCache.Setup(c => c.TryGetValue(CacheKeys.HomeworkAssignments, out cachedValue)).Returns(false);
        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId)).ReturnsAsync((HomeworkAssignment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.HomeworkAssignmentService.GetAssignmentByIdAsync(assignmentId));
        Assert.Contains(assignmentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task DeleteAssignmentAsync_ShouldDeleteAssignmentAndInvalidateCache_WhenAssignmentExists()
    {
        // Arrange
        var assignmentId = Guid.NewGuid();
        var existingAssignment = HomeworkAssignmentServiceTestFixture.CreateSampleHomeworkAssignment();
        existingAssignment.Id = assignmentId;

        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId)).ReturnsAsync(existingAssignment);
        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.Delete(existingAssignment)).Verifiable();
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        await _fixture.HomeworkAssignmentService.DeleteAssignmentAsync(assignmentId);

        // Assert
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.GetByIdAsync(assignmentId), Times.Once);
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.Delete(existingAssignment), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.HomeworkAssignments), Times.Once);
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_ShouldReturnAllAssignments_WhenCalled()
    {
        // Arrange
        _fixture.ResetMocks();
        var assignments = new List<HomeworkAssignment>
        {
            HomeworkAssignmentServiceTestFixture.CreateSampleHomeworkAssignment(),
            HomeworkAssignmentServiceTestFixture.CreateSampleHomeworkAssignment(),
        };
        var assignmentDtos = assignments.Select(_ => HomeworkAssignmentServiceTestFixture.CreateSampleHomeworkAssignmentDto()).ToList();

        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(assignments);
        _fixture.MockMapper.Setup(m => m.Map<IEnumerable<HomeworkAssignmentDto>>(assignments)).Returns(assignmentDtos);

        // Act
        var result = await _fixture.HomeworkAssignmentService.GetAllAssignmentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assignmentDtos.Count, result.Count());
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.GetAllAsync(), Times.Once);
        _fixture.MockMapper.Verify(m => m.Map<IEnumerable<HomeworkAssignmentDto>>(assignments), Times.Once);
    }

    [Fact]
    public async Task SubmitAssignmentAsync_ShouldUpdateAssignmentAndInvalidateCache_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var assignmentId = Guid.NewGuid();
        var existingAssignment = HomeworkAssignmentServiceTestFixture.CreateSampleHomeworkAssignment();
        existingAssignment.Id = assignmentId;
        var submitDto = HomeworkAssignmentServiceTestFixture.CreateSampleSubmitHomeworkAssignmentDto();

        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId)).ReturnsAsync(existingAssignment);
        _fixture.MockMapper.Setup(m => m.Map(submitDto, existingAssignment));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _fixture.HomeworkAssignmentService.SubmitAssignmentAsync(assignmentId, submitDto);

        // Assert
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.GetByIdAsync(assignmentId), Times.Once);
        _fixture.MockMapper.Verify(m => m.Map(submitDto, existingAssignment), Times.Once);
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.Update(existingAssignment), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.HomeworkAssignments), Times.Once);
    }

    [Fact]
    public async Task SubmitAssignmentAsync_ShouldThrowKeyNotFoundException_WhenAssignmentNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var assignmentId = Guid.NewGuid();
        var submitDto = HomeworkAssignmentServiceTestFixture.CreateSampleSubmitHomeworkAssignmentDto();

        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId)).ReturnsAsync((HomeworkAssignment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _fixture.HomeworkAssignmentService.SubmitAssignmentAsync(assignmentId, submitDto));
        Assert.Contains(assignmentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GradeAssignmentAsync_ShouldUpdateAssignmentAndInvalidateCache_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var assignmentId = Guid.NewGuid();
        var existingAssignment = HomeworkAssignmentServiceTestFixture.CreateSampleHomeworkAssignment();
        existingAssignment.Id = assignmentId;
        var gradeDto = HomeworkAssignmentServiceTestFixture.CreateSampleGradeHomeworkAssignmentDto();

        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId)).ReturnsAsync(existingAssignment);
        _fixture.MockMapper.Setup(m => m.Map(gradeDto, existingAssignment));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _fixture.HomeworkAssignmentService.GradeAssignmentAsync(assignmentId, gradeDto);

        // Assert
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.GetByIdAsync(assignmentId), Times.Once);
        _fixture.MockMapper.Verify(m => m.Map(gradeDto, existingAssignment), Times.Once);
        _fixture.MockHomeworkAssignmentRepository.Verify(r => r.Update(existingAssignment), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _fixture.MockMemoryCache.Verify(c => c.Remove(CacheKeys.HomeworkAssignments), Times.Once);
    }

    [Fact]
    public async Task GradeAssignmentAsync_ShouldThrowKeyNotFoundException_WhenAssignmentNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var assignmentId = Guid.NewGuid();
        var gradeDto = HomeworkAssignmentServiceTestFixture.CreateSampleGradeHomeworkAssignmentDto();

        _fixture.MockHomeworkAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId)).ReturnsAsync((HomeworkAssignment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _fixture.HomeworkAssignmentService.GradeAssignmentAsync(assignmentId, gradeDto));
        Assert.Contains(assignmentId.ToString(), exception.Message);
    }
}