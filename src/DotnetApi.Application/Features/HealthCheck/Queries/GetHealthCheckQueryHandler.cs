using DotnetApi.Application.Features.HealthCheck.Options;
using DotnetApi.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotnetApi.Application.Features.HealthCheck.Queries.GetHealthCheck;

/// <summary>
/// Handles the GetHealthCheckQuery and returns the current health status
/// </summary>
public class GetHealthCheckQueryHandler : IRequestHandler<GetHealthCheckQuery, GetHealthCheckQueryResponse>
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly HealthCheckOptions _options;

    public GetHealthCheckQueryHandler(
        IHealthCheckService healthCheckService,
        IOptions<HealthCheckOptions> options)
    {
        _healthCheckService = healthCheckService;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<GetHealthCheckQueryResponse> Handle(
        GetHealthCheckQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _healthCheckService.CheckHealthAsync(cancellationToken);

        return new GetHealthCheckQueryResponse(
            Status: result.Status,
            Timestamp: result.Timestamp,
            Version: _options.Version,
            Environment: _options.Environment,
            Components: result.Components,
            ErrorMessage: result.ErrorMessage
        );
    }
}
