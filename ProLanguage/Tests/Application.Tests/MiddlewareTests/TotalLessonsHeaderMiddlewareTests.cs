using Application.Interfaces;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Application.Tests.MiddlewareTests;

public class TotalLessonsHeaderMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILessonService> _mockLessonService;
    private readonly TotalLessonsHeaderMiddleware _middleware;

    public TotalLessonsHeaderMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLessonService = new Mock<ILessonService>();
        _middleware = new TotalLessonsHeaderMiddleware(_mockNext.Object);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallLessonService_WhenInvoked()
    {
        // Arrange
        const int totalLessons = 42;
        var context = new DefaultHttpContext();

        _mockLessonService.Setup(s => s.GetTotalLessonsCountAsync()).ReturnsAsync(totalLessons);
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context, _mockLessonService.Object);

        // Assert
        _mockLessonService.Verify(s => s.GetTotalLessonsCountAsync(), Times.Once);
        _mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallService_WhenServiceReturnsZero()
    {
        // Arrange
        const int totalLessons = 0;
        var context = new DefaultHttpContext();

        _mockLessonService.Setup(s => s.GetTotalLessonsCountAsync()).ReturnsAsync(totalLessons);
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context, _mockLessonService.Object);

        // Assert
        _mockLessonService.Verify(s => s.GetTotalLessonsCountAsync(), Times.Once);
        _mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware_EvenIfServiceThrows()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _mockLessonService.Setup(s => s.GetTotalLessonsCountAsync()).ThrowsAsync(new Exception("Service error"));
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(context, _mockLessonService.Object));

        // Verify that the service was called but next middleware was not called due to exception
        _mockLessonService.Verify(s => s.GetTotalLessonsCountAsync(), Times.Once);
        _mockNext.Verify(n => n(context), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRegisterOnStartingCallback_BeforeCallingNext()
    {
        // Arrange
        const int totalLessons = 15;
        var context = new DefaultHttpContext();

        _mockLessonService.Setup(s => s.GetTotalLessonsCountAsync()).ReturnsAsync(totalLessons);
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context, _mockLessonService.Object);

        // Assert
        _mockLessonService.Verify(s => s.GetTotalLessonsCountAsync(), Times.Once);
        _mockNext.Verify(n => n(context), Times.Once);
    }
}