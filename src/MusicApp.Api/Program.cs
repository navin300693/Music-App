var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();

var app = builder.Build();

// Standard Health Check for Azure/Kubernetes
app.MapHealthChecks("/health");

// Your main entry point
app.MapGet("/", () => new
{
    Status = "Online",
    Project = "Music App API",
    Version = "1.0.0"
});

app.Run();