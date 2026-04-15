using Application.DTOs.Course;
using Application.Tests.Fixtures;
using Domain.Entities;
using Moq;

namespace Application.Tests.ServicesTests;

public class CourseServiceTests(CourseServiceTestFixture fixture) : IClassFixture<CourseServiceTestFixture>
{
    private readonly CourseServiceTestFixture _fixture = fixture;

    [Fact]
    public async Task CreateCourseAsync_ShouldCreateCourse_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var createDto = CourseServiceTestFixture.CreateSampleCreateCourseDto();
        var course = CourseServiceTestFixture.CreateSampleCourse();

        _fixture.MockCourseRepository.Setup(r => r.TitleExistsAsync(createDto.Title, null)).ReturnsAsync(false);
        _fixture.MockMapper.Setup(m => m.Map<Course>(createDto)).Returns(course);
        _fixture.MockCourseRepository.Setup(r => r.AddAsync(It.IsAny<Course>())).Returns(Task.FromResult(course));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        var result = await _fixture.CourseService.CreateCourseAsync(createDto);

        // Assert
        Assert.Equal(course.Id, result);
        _fixture.MockCourseRepository.Verify(r => r.TitleExistsAsync(createDto.Title, null), Times.Once);
        _fixture.MockCourseRepository.Verify(r => r.AddAsync(course), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCourseAsync_ShouldThrowInvalidOperationException_WhenTitleAlreadyExists()
    {
        // Arrange
        _fixture.ResetMocks();
        var createDto = CourseServiceTestFixture.CreateSampleCreateCourseDto();

        _fixture.MockCourseRepository.Setup(r => r.TitleExistsAsync(createDto.Title, null)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.CourseService.CreateCourseAsync(createDto));
        Assert.Contains(createDto.Title, exception.Message);
        _fixture.MockCourseRepository.Verify(r => r.AddAsync(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task GetCourseByIdAsync_ShouldReturnCourse_WhenCourseExists()
    {
        // Arrange
        _fixture.ResetMocks();
        var courseId = Guid.NewGuid();
        var course = CourseServiceTestFixture.CreateSampleCourse();
        course.Id = courseId;
        var courseDto = CourseServiceTestFixture.CreateSampleCourseDto();

        _fixture.MockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);
        _fixture.MockMapper.Setup(m => m.Map<CourseDto>(course)).Returns(courseDto);

        // Act
        var result = await _fixture.CourseService.GetCourseByIdAsync(courseId);

        // Assert
        Assert.Equal(courseDto, result);
        _fixture.MockCourseRepository.Verify(r => r.GetByIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task GetCourseByIdAsync_ShouldThrowKeyNotFoundException_WhenCourseNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var courseId = Guid.NewGuid();

        _fixture.MockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.CourseService.GetCourseByIdAsync(courseId));
        Assert.Contains(courseId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetCourseByTitleAsync_ShouldReturnCourse_WhenCourseExists()
    {
        // Arrange
        _fixture.ResetMocks();
        var title = "Test Course";
        var course = CourseServiceTestFixture.CreateSampleCourse();
        course.Title = title;
        var courseDto = CourseServiceTestFixture.CreateSampleCourseDto();

        _fixture.MockCourseRepository.Setup(r => r.GetByTitleAsync(title)).ReturnsAsync(course);
        _fixture.MockMapper.Setup(m => m.Map<CourseDto>(course)).Returns(courseDto);

        // Act
        var result = await _fixture.CourseService.GetCourseByTitleAsync(title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(courseDto, result);
        _fixture.MockCourseRepository.Verify(r => r.GetByTitleAsync(title), Times.Once);
    }

    [Fact]
    public async Task GetCourseByTitleAsync_ShouldReturnNull_WhenCourseNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var title = "Nonexistent Course";

        _fixture.MockCourseRepository.Setup(r => r.GetByTitleAsync(title)).ReturnsAsync((Course?)null);

        // Act
        var result = await _fixture.CourseService.GetCourseByTitleAsync(title);

        // Assert
        Assert.Null(result);
        _fixture.MockCourseRepository.Verify(r => r.GetByTitleAsync(title), Times.Once);
    }

    [Fact]
    public async Task GetAllCoursesAsync_ShouldReturnAllCourses()
    {
        // Arrange
        _fixture.ResetMocks();
        var courses = new List<Course>
        {
            CourseServiceTestFixture.CreateSampleCourse(),
            CourseServiceTestFixture.CreateSampleCourse(),
            CourseServiceTestFixture.CreateSampleCourse(),
        };
        var courseDtos = new List<CourseDto>
        {
            CourseServiceTestFixture.CreateSampleCourseDto(),
            CourseServiceTestFixture.CreateSampleCourseDto(),
            CourseServiceTestFixture.CreateSampleCourseDto(),
        };

        _fixture.MockCourseRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(courses);
        _fixture.MockMapper.Setup(m => m.Map<IEnumerable<CourseDto>>(courses)).Returns(courseDtos);

        // Act
        var result = await _fixture.CourseService.GetAllCoursesAsync();

        // Assert
        Assert.Equal(3, result.Count());
        _fixture.MockCourseRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAvailableCoursesAsync_ShouldReturnAvailableCourses_WithoutExcludingLesson()
    {
        // Arrange
        _fixture.ResetMocks();
        var courses = new List<Course>
        {
            CourseServiceTestFixture.CreateSampleCourse(),
            CourseServiceTestFixture.CreateSampleCourse(),
        };
        var courseDtos = new List<CourseDto>
        {
            CourseServiceTestFixture.CreateSampleCourseDto(),
            CourseServiceTestFixture.CreateSampleCourseDto(),
        };

        _fixture.MockCourseRepository.Setup(r => r.GetAvailableCoursesAsync(null)).ReturnsAsync(courses);
        _fixture.MockMapper.Setup(m => m.Map<IEnumerable<CourseDto>>(courses)).Returns(courseDtos);

        // Act
        var result = await _fixture.CourseService.GetAvailableCoursesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _fixture.MockCourseRepository.Verify(r => r.GetAvailableCoursesAsync(null), Times.Once);
    }

    [Fact]
    public async Task GetAvailableCoursesAsync_ShouldReturnAvailableCourses_WithExcludingLesson()
    {
        // Arrange
        _fixture.ResetMocks();
        var excludeLessonId = Guid.NewGuid();
        var courses = new List<Course>
        {
            CourseServiceTestFixture.CreateSampleCourse(),
            CourseServiceTestFixture.CreateSampleCourse(),
        };
        var courseDtos = new List<CourseDto>
        {
            CourseServiceTestFixture.CreateSampleCourseDto(),
            CourseServiceTestFixture.CreateSampleCourseDto(),
        };

        _fixture.MockCourseRepository.Setup(r => r.GetAvailableCoursesAsync(excludeLessonId)).ReturnsAsync(courses);
        _fixture.MockMapper.Setup(m => m.Map<IEnumerable<CourseDto>>(courses)).Returns(courseDtos);

        // Act
        var result = await _fixture.CourseService.GetAvailableCoursesAsync(excludeLessonId);

        // Assert
        Assert.Equal(2, result.Count());
        _fixture.MockCourseRepository.Verify(r => r.GetAvailableCoursesAsync(excludeLessonId), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldUpdateCourse_WhenValidDtoProvided()
    {
        // Arrange
        _fixture.ResetMocks();
        var courseId = Guid.NewGuid();
        var existingCourse = CourseServiceTestFixture.CreateSampleCourse();
        existingCourse.Id = courseId;
        var updateDto = CourseServiceTestFixture.CreateSampleUpdateCourseDto(courseId);

        _fixture.MockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(existingCourse);
        _fixture.MockCourseRepository.Setup(r => r.TitleExistsAsync(updateDto.Title!, courseId)).ReturnsAsync(false);
        _fixture.MockMapper.Setup(m => m.Map(updateDto, existingCourse));
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _fixture.CourseService.UpdateCourseAsync(updateDto);

        // Assert
        _fixture.MockCourseRepository.Verify(r => r.GetByIdAsync(courseId), Times.Once);
        _fixture.MockCourseRepository.Verify(r => r.TitleExistsAsync(updateDto.Title!, courseId), Times.Once);
        _fixture.MockMapper.Verify(m => m.Map(updateDto, existingCourse), Times.Once);
        _fixture.MockCourseRepository.Verify(r => r.Update(existingCourse), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldThrowKeyNotFoundException_WhenCourseNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var courseId = Guid.NewGuid();
        var updateDto = CourseServiceTestFixture.CreateSampleUpdateCourseDto(courseId);

        _fixture.MockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.CourseService.UpdateCourseAsync(updateDto));
        Assert.Contains(courseId.ToString(), exception.Message);
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldThrowInvalidOperationException_WhenTitleAlreadyExists()
    {
        // Arrange
        _fixture.ResetMocks();
        var courseId = Guid.NewGuid();
        var existingCourse = CourseServiceTestFixture.CreateSampleCourse();
        existingCourse.Id = courseId;
        var updateDto = CourseServiceTestFixture.CreateSampleUpdateCourseDto(courseId);

        _fixture.MockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(existingCourse);
        _fixture.MockCourseRepository.Setup(r => r.TitleExistsAsync(updateDto.Title!, courseId)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.CourseService.UpdateCourseAsync(updateDto));
        Assert.Contains(updateDto.Title!, exception.Message);
        _fixture.MockCourseRepository.Verify(r => r.Update(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCourseAsync_ShouldDeleteCourse_WhenCourseExists()
    {
        // Arrange
        _fixture.ResetMocks();
        var courseId = Guid.NewGuid();
        var existingCourse = CourseServiceTestFixture.CreateSampleCourse();
        existingCourse.Id = courseId;

        _fixture.MockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(existingCourse);
        _fixture.MockCourseRepository.Setup(r => r.Delete(existingCourse)).Verifiable();
        _fixture.MockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

        // Act
        await _fixture.CourseService.DeleteCourseAsync(courseId);

        // Assert
        _fixture.MockCourseRepository.Verify(r => r.GetByIdAsync(courseId), Times.Once);
        _fixture.MockCourseRepository.Verify(r => r.Delete(existingCourse), Times.Once);
        _fixture.MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsync_ShouldThrowKeyNotFoundException_WhenCourseNotFound()
    {
        // Arrange
        _fixture.ResetMocks();
        var courseId = Guid.NewGuid();

        _fixture.MockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixture.CourseService.DeleteCourseAsync(courseId));
        Assert.Contains(courseId.ToString(), exception.Message);
    }
}
