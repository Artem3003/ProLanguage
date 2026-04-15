using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Middleware;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger = logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            LogExceptionDetails(context, ex);
            await HandleExceptionAsync(context, ex);
        }
    }

    private void LogExceptionDetails(HttpContext context, Exception exception)
    {
        var exceptionDetails = new
        {
            ExceptionType = exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            RequestPath = context.Request.Path,
            RequestMethod = context.Request.Method,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            RequestId = context.TraceIdentifier,
            TimeStamp = DateTime.UtcNow,
            InnerExceptions = GetInnerExceptions(exception),
        };

        _logger.LogError(exception, "Unhandled exception occurred. Details: {@ExceptionDetails}", exceptionDetails);
    }

    private static List<string> GetInnerExceptions(Exception exception)
    {
        var innerExceptions = new List<string>();
        var innerException = exception.InnerException;

        while (innerException != null)
        {
            innerExceptions.Add($"{innerException.GetType().Name}: {innerException.Message}");
            innerException = innerException.InnerException;
        }

        return innerExceptions;
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = GetErrorResponse(exception);
        context.Response.StatusCode = statusCode;

        var response = new
        {
            StatusCode = statusCode,
            Message = message,
            Details = exception.Message,
            Timestamp = DateTime.UtcNow,
            TraceId = context.TraceIdentifier,
        };

        var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private static (int StatusCode, string Message) GetErrorResponse(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "The requested resource could not be found."),
            ArgumentNullException => ((int)HttpStatusCode.BadRequest, "Required information is missing. Please provide all necessary data."),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Invalid request. Please check your input and try again."),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, "The requested operation cannot be completed at this time."),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "You don't have permission to access this resource."),
            TimeoutException => ((int)HttpStatusCode.RequestTimeout, "The request took too long to process. Please try again."),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later or contact support if the problem persists."),
        };
    }
}
