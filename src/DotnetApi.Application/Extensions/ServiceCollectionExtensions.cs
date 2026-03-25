using DotnetApi.Application.Features.HealthCheck.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetApi.Application.Extensions;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.Configure<HealthCheckOptions>(
            configuration.GetSection(HealthCheckOptions.SectionName));

        return services;
    }
}
