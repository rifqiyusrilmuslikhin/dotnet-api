using DotnetApi.Api.Exceptions;
using DotnetApi.Application.Extensions;
using DotnetApi.Infrastructure.Extensions;
using Serilog;

namespace DotnetApi.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Registers all API, Application, and Infrastructure services
    /// </summary>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilogLogging();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);

        return builder;
    }

    /// <summary>
    /// Configures the HTTP request pipeline middlewares
    /// </summary>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
            });
        }

        app.UseExceptionHandler();
        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
