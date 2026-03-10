using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestInfo = await CaptureRequestInfoAsync(context);

        // Buffer the response to capture content
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var responseContent = await CaptureResponseContentAsync(responseBody);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            LogRequestDetails(context, requestInfo, responseContent, stopwatch.ElapsedMilliseconds);
        }
    }

    private static async Task<string> CaptureRequestInfoAsync(HttpContext context)
    {
        var request = context.Request;

        request.EnableBuffering();

        var requestContent = string.Empty;
        if (request.ContentLength is > 0 and < 10000)
        {
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestContent = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        return requestContent;
    }

    private static async Task<string> CaptureResponseContentAsync(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);

        if (responseBody.Length is 0 or > 10000)
        {
            return responseBody.Length > 10000 ? "[Response too large to log]" : string.Empty;
        }

        using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
        var content = await reader.ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        return content;
    }

    private void LogRequestDetails(
        HttpContext context,
        string requestContent,
        string responseContent,
        long elapsedMilliseconds)
    {
        var requestDetails = new
        {
            Timestamp = DateTime.UtcNow,
            RequestId = context.TraceIdentifier,
            ClientIpAddress = GetClientIpAddress(context),
            Method = context.Request.Method,
            TargetUrl = GetFullUrl(context.Request),
            RequestHeaders = GetHeaders(context.Request.Headers),
            RequestContent = SanitizeContent(requestContent),
            ResponseStatusCode = context.Response.StatusCode,
            ResponseHeaders = GetHeaders(context.Response.Headers),
            ResponseContent = SanitizeContent(responseContent),
            ElapsedTimeMs = elapsedMilliseconds,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            Referer = context.Request.Headers.Referer.ToString(),
        };

        _logger.LogInformation(
            "HTTP Request completed. Method: {Method}, URL: {TargetUrl}, Status: {StatusCode}, " +
            "IP: {ClientIpAddress}, Elapsed: {ElapsedTimeMs}ms, RequestId: {RequestId}",
            requestDetails.Method,
            requestDetails.TargetUrl,
            requestDetails.ResponseStatusCode,
            requestDetails.ClientIpAddress,
            requestDetails.ElapsedTimeMs,
            requestDetails.RequestId);

        _logger.LogDebug("Request Details: {@RequestDetails}", requestDetails);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        return !string.IsNullOrEmpty(realIp) ? realIp : context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static string GetFullUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
    }

    private static Dictionary<string, string> GetHeaders(IHeaderDictionary headers)
    {
        return headers.Where(h => !IsSensitiveHeader(h.Key))
            .ToDictionary(
                h => h.Key,
                h => string.Join(", ", h.Value.AsEnumerable()));
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "Authorization",
            "Cookie",
            "Set-Cookie",
            "X-API-Key",
            "X-Auth-Token",
        };

        return sensitiveHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    private static string SanitizeContent(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        // Remove sensitive information patterns
        var sensitivePatterns = new[]
        {
            @"""password""\s*:\s*""[^""]*""",
            @"""token""\s*:\s*""[^""]*""",
            @"""apiKey""\s*:\s*""[^""]*""",
            @"""secret""\s*:\s*""[^""]*""",
        };

        var sanitized = content;
        foreach (var pattern in sensitivePatterns)
        {
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized,
                pattern,
                match => match.Value[..(match.Value.IndexOf(':') + 1)] + " \"[REDACTED]\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return sanitized.Length > 1000 ? sanitized[..1000] + "..." : sanitized;
    }
}