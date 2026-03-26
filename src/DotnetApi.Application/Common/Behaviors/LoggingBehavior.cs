using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DotnetApi.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs all requests and warns
/// when a handler takes longer than the threshold (500ms).
/// </summary>
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int PerformanceThresholdMs = 500;
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > PerformanceThresholdMs)
        {
            _logger.LogWarning(
                "Long running request: {RequestName} took {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
