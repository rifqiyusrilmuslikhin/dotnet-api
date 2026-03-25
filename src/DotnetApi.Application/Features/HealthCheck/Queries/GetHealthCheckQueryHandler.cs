using DotnetApi.Application.Features.HealthCheck.Options;
using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotnetApi.Application.Features.HealthCheck.Queries.GetHealthCheck;

/// <summary>
/// Handles the GetHealthCheckQuery and returns the current health status
/// </summary>
public class GetHealthCheckQueryHandler : IRequestHandler<GetHealthCheckQuery, GetHealthCheckQueryResponse>
{
    private readonly HealthCheckOptions _options;

    public GetHealthCheckQueryHandler(IOptions<HealthCheckOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<GetHealthCheckQueryResponse> Handle(
        GetHealthCheckQuery request,
        CancellationToken cancellationToken)
    {
        var components = new Dictionary<string, ComponentHealthInfo>();

        try
        {
            await CheckApplicationHealthAsync(components, cancellationToken);

            var overallStatus = DetermineOverallStatus(components);

            return new GetHealthCheckQueryResponse(
                Status: overallStatus.ToString(),
                Timestamp: DateTime.UtcNow,
                Version: _options.Version,
                Environment: _options.Environment,
                Components: components
            );
        }
        catch (Exception ex)
        {
            return new GetHealthCheckQueryResponse(
                Status: HealthStatus.Unhealthy.ToString(),
                Timestamp: DateTime.UtcNow,
                Version: _options.Version,
                Environment: _options.Environment,
                Components: components,
                ErrorMessage: ex.Message
            );
        }
    }

    /// <summary>
    /// Checks the health of all application components
    /// </summary>
    private async Task CheckApplicationHealthAsync(
        Dictionary<string, ComponentHealthInfo> components,
        CancellationToken cancellationToken)
    {
        await CheckAppStatusAsync(components, cancellationToken);
        await CheckMemoryAsync(components);
    }

    private static async Task CheckAppStatusAsync(
        Dictionary<string, ComponentHealthInfo> components,
        CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await Task.Delay(10, cancellationToken);
            sw.Stop();

            components["Application"] = new ComponentHealthInfo
            {
                Status = HealthStatus.Healthy.ToString(),
                Description = "Application is running normally",
                ResponseTimeMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            components["Application"] = new ComponentHealthInfo
            {
                Status = HealthStatus.Unhealthy.ToString(),
                Description = "Application health check failed",
                ResponseTimeMs = sw.ElapsedMilliseconds,
                ErrorMessage = ex.Message
            };
        }
    }

    private static Task CheckMemoryAsync(Dictionary<string, ComponentHealthInfo> components)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var memoryUsage = GC.GetTotalMemory(false);
            const long maxMemoryThreshold = 2_000_000_000; // 2GB

            var status = memoryUsage < maxMemoryThreshold
                ? HealthStatus.Healthy
                : HealthStatus.Degraded;

            sw.Stop();

            components["Memory"] = new ComponentHealthInfo
            {
                Status = status.ToString(),
                Description = $"Memory usage: {FormatBytes(memoryUsage)}",
                ResponseTimeMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            components["Memory"] = new ComponentHealthInfo
            {
                Status = HealthStatus.Unhealthy.ToString(),
                Description = "Memory check failed",
                ResponseTimeMs = sw.ElapsedMilliseconds,
                ErrorMessage = ex.Message
            };
        }

        return Task.CompletedTask;
    }

    private static HealthStatus DetermineOverallStatus(Dictionary<string, ComponentHealthInfo> components)
    {
        if (components.Values.Any(c => c.Status == HealthStatus.Unhealthy.ToString()))
            return HealthStatus.Unhealthy;

        if (components.Values.Any(c => c.Status == HealthStatus.Degraded.ToString()))
            return HealthStatus.Degraded;

        return HealthStatus.Healthy;
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}