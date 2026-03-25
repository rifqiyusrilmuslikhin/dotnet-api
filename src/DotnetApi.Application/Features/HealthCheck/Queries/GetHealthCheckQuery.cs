using MediatR;

namespace DotnetApi.Application.Features.HealthCheck.Queries.GetHealthCheck;

/// <summary>
/// Query to get the current health status of the application
/// </summary>
public record GetHealthCheckQuery : IRequest<GetHealthCheckQueryResponse>;