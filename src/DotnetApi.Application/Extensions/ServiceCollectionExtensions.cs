using DotnetApi.Application.Common.Behaviors;
using DotnetApi.Application.Features.HealthCheck.Options;
using FluentValidation;
using MediatR;
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

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.Configure<HealthCheckOptions>(
            configuration.GetSection(HealthCheckOptions.SectionName));

        return services;
    }
}
