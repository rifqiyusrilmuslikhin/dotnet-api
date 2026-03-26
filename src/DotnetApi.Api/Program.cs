using DotnetApi.Api.Extensions;
using Serilog;

SerilogExtensions.AddBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServices();

    var app = builder.Build();
    app.ConfigurePipeline();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

