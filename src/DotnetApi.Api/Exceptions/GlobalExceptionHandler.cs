using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;

namespace DotnetApi.Api.Exceptions;

/// <summary>
/// Global exception handler that maps domain and application exceptions to HTTP responses
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                ex.Message,
                ex.Errors
            ),
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                ex.Message,
                (IDictionary<string, string[]>?)null
            ),
            DomainException ex => (
                StatusCodes.Status400BadRequest,
                "Business Rule Violation",
                ex.Message,
                (IDictionary<string, string[]>?)null
            ),
            UnauthorizedAccessException ex => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                ex.Message,
                (IDictionary<string, string[]>?)null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.",
                (IDictionary<string, string[]>?)null
            )
        };

        using (LogContext.PushProperty("TraceId", traceId))
        {
            if (statusCode == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            else
                _logger.LogWarning(exception, "Handled exception [{Status}]: {Message}", statusCode, exception.Message);
        }

        var response = new ErrorResponse
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            TraceId = traceId,
            Errors = errors
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
