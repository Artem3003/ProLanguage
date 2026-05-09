using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Middleware;

public class TotalLessonsHeaderMiddleware(RequestDelegate next, ILogger<TotalLessonsHeaderMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<TotalLessonsHeaderMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, ILessonService lessonService)
    {
        if (!context.Request.Path.StartsWithSegments("/lessons", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        int? totalLessons = null;
        try
        {
            totalLessons = await lessonService.GetTotalLessonsCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load total lessons count for path {Path}", context.Request.Path);
        }

        if (totalLessons.HasValue)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["x-total-number-of-lessons"] = totalLessons.Value.ToString();
                return Task.CompletedTask;
            });
        }

        await _next(context);
    }
}
