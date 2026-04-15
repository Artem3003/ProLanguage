using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Middleware;

public class TotalLessonsHeaderMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ILessonService lessonService)
    {
        var totalLessons = await lessonService.GetTotalLessonsCountAsync();
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["x-total-number-of-lessons"] = totalLessons.ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }
}