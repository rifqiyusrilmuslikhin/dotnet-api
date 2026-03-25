namespace DotnetApi.Domain.Entities;

/// <summary>
/// Represents the health status of the application
/// </summary>
public class HealthCheckEntity
{
    /// <summary>
    /// Gets the status of the application (Healthy, Degraded, or Unhealthy)
    /// </summary>
    public string Status { get; init; } = null!;

    /// <summary>
    /// Gets the timestamp of the health check
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the version of the application
    /// </summary>
    public string Version { get; init; } = null!;

    /// <summary>
    /// Gets the environment name (Development, Staging, Production)
    /// </summary>
    public string Environment { get; init; } = null!;

    /// <summary>
    /// Gets detailed information about each component's health
    /// </summary>
    public Dictionary<string, ComponentHealthInfo> Components { get; init; } = new();

    /// <summary>
    /// Gets optional error message if health check failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Represents the health information of a specific component
/// </summary>
public class ComponentHealthInfo
{
    /// <summary>
    /// Gets the status of the component (Healthy, Degraded, or Unhealthy)
    /// </summary>
    public string Status { get; init; } = null!;

    /// <summary>
    /// Gets optional description about the component
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; init; }

    /// <summary>
    /// Gets optional error message if the component is unhealthy
    /// </summary>
    public string? ErrorMessage { get; init; }
}
