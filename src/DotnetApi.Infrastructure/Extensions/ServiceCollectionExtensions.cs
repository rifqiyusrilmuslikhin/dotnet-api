using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Infrastructure.Options;
using DotnetApi.Infrastructure.Persistence;
using DotnetApi.Infrastructure.Repositories;
using DotnetApi.Infrastructure.Services;

namespace DotnetApi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Infrastructure services including PostgreSQL DbContext and JWT authentication
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
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

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<IHealthCheckService, HealthCheckService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // JWT options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // File storage options
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

        // Authentication & Authorization
        var jwtSecret = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("'Jwt:SecretKey' is not configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Suppress default response
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = MediaTypeNames.Application.Json;

                        var response = new
                        {
                            status = StatusCodes.Status401Unauthorized,
                            title = "Unauthorized",
                            detail = "Access token is missing or invalid.",
                            traceId = context.HttpContext.TraceIdentifier
                        };

                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(response, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = MediaTypeNames.Application.Json;

                        var response = new
                        {
                            status = StatusCodes.Status403Forbidden,
                            title = "Forbidden",
                            detail = "You do not have permission to access this resource.",
                            traceId = context.HttpContext.TraceIdentifier
                        };

                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(response, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }));
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
