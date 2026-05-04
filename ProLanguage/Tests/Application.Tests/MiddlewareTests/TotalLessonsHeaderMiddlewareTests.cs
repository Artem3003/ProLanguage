using Application.Interfaces;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.MiddlewareTests;

public class TotalLessonsHeaderMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILessonService> _mockLessonService;
    private readonly Mock<ILogger<TotalLessonsHeaderMiddleware>> _mockLogger;
    private readonly TotalLessonsHeaderMiddleware _middleware;

    public TotalLessonsHeaderMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLessonService = new Mock<ILessonService>();
        _mockLogger = new Mock<ILogger<TotalLessonsHeaderMiddleware>>();
        _middleware = new TotalLessonsHeaderMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallLessonService_ForLessonsPath()
    {
        // Arrange
        const int totalLessons = 42;
        var context = new DefaultHttpContext();
        context.Request.Path = "/lessons/filter";

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
        context.Request.Path = "/lessons";

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
        context.Request.Path = "/lessons";
        _mockLessonService.Setup(s => s.GetTotalLessonsCountAsync()).ThrowsAsync(new Exception("Service error"));
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context, _mockLessonService.Object);

        // Verify that the request continues even if header enrichment fails
        _mockLessonService.Verify(s => s.GetTotalLessonsCountAsync(), Times.Once);
        _mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRegisterOnStartingCallback_BeforeCallingNext()
    {
        // Arrange
        const int totalLessons = 15;
        var context = new DefaultHttpContext();
        context.Request.Path = "/lessons";

        _mockLessonService.Setup(s => s.GetTotalLessonsCountAsync()).ReturnsAsync(totalLessons);
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context, _mockLessonService.Object);

        // Assert
        _mockLessonService.Verify(s => s.GetTotalLessonsCountAsync(), Times.Once);
        _mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSkipLessonService_ForNonLessonsPath()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/courses/filter";
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context, _mockLessonService.Object);

        // Assert
        _mockLessonService.Verify(s => s.GetTotalLessonsCountAsync(), Times.Never);
        _mockNext.Verify(n => n(context), Times.Once);
    }
}
