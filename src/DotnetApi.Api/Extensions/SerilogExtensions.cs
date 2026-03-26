using Serilog;

namespace DotnetApi.Api.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    /// Configures the bootstrap logger used before the host is fully built
    /// </summary>
    public static void AddBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Configures Serilog to read from appsettings.json and enrich from DI/LogContext
    /// </summary>
    public static IHostBuilder UseSerilogLogging(this IHostBuilder host)
    {
        return host.UseSerilog((context, services, config) =>
            config
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());
    }
}
