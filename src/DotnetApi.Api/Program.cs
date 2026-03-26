using DotnetApi.Application.Extensions;
using DotnetApi.Infrastructure.Extensions; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Application services (MediatR, Validators, Behaviors)
builder.Services.AddApplication(builder.Configuration);

// Register Infrastructure services (PostgreSQL DbContext, Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => 
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
