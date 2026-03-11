using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Health checks — liveness and readiness probes for Kubernetes
builder.Services.AddHealthChecks();
// To add readiness-specific checks (e.g., database connectivity), tag them:
// builder.Services.AddHealthChecks()
//     .AddCheck<MyDbHealthCheck>("db", tags: ["ready"]);

var app = builder.Build();


app.MapOpenApi();
app.MapScalarApiReference();

// Liveness probe: returns 200 if the process is alive (no checks executed)
app.MapHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness probe: runs all checks tagged "ready" (add checks above with tags: ["ready"])
app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
