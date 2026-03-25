using DotnetApi.Domain.Entities;

namespace DotnetApi.Domain.Interfaces;

/// <summary>
/// Defines the contract for health check operations
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Performs a health check on the application and its components
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    Task<HealthCheckEntity> CheckHealthAsync(CancellationToken cancellationToken = default);
}
