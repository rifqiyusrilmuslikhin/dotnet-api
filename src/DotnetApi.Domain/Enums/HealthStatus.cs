namespace DotnetApi.Domain.Enums;

/// <summary>
/// Enum representing the health status levels
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// The application is healthy and operating normally
    /// </summary>
    Healthy,

    /// <summary>
    /// The application is operational but with some issues
    /// </summary>
    Degraded,

    /// <summary>
    /// The application is unhealthy and cannot operate properly
    /// </summary>
    Unhealthy
}
