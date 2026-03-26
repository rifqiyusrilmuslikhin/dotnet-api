using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using DotnetApi.Infrastructure.Persistence;

namespace DotnetApi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Infrastructure services including PostgreSQL DbContext
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options
                .UseNpgsql(
                    connectionString,
                    npgsql => npgsql
                        .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                        .CommandTimeout(30)
                )
                .UseSnakeCaseNamingConvention();
        });

        // Register repositories, interfaces, and services

        return services;
    }
}
