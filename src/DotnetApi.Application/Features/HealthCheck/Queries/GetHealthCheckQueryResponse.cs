using DotnetApi.Domain.Entities;

namespace DotnetApi.Application.Features.HealthCheck.Queries.GetHealthCheck;

/// <summary>
/// Response DTO for the GetHealthCheckQuery
/// </summary>
public record GetHealthCheckQueryResponse(
    string Status,
    DateTime Timestamp,
    string Version,
    string Environment,
    Dictionary<string, ComponentHealthInfo> Components,
    string? ErrorMessage = null
);
