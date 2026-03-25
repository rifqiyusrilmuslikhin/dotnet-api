namespace DotnetApi.Application.Features.HealthCheck.Options;

/// <summary>
/// Configuration options for the health check
/// </summary>
public class HealthCheckOptions
{
    public const string SectionName = "HealthCheck";

    /// <summary>
    /// The application version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// The environment name
    /// </summary>
    public string Environment { get; set; } = "Production";
}
