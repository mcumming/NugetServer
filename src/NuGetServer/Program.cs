using NuGetServer.Configuration;
using NuGetServer.Endpoints;
using NuGetServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure options from appsettings.json and environment variables
builder.Services.Configure<NuGetServerOptions>(
    builder.Configuration.GetSection(NuGetServerOptions.SectionName));

// Add services to the container
builder.Services.AddSingleton<IPackageService, FileSystemPackageService>();

// Add OpenAPI/Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v3", new()
    {
        Title = "NuGet Server v3 API",
        Version = "v3",
        Description = "A lightweight NuGet v3 protocol server implementation"
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure telemetry (for OpenTelemetry support in future)
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});

var app = builder.Build();

// Ensure packages directory exists
var packagesPath = builder.Configuration.GetValue<string>($"{NuGetServerOptions.SectionName}:PackagesPath") ?? "/packages";
Directory.CreateDirectory(packagesPath);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v3/swagger.json", "NuGet Server v3 API");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpLogging();

// Map NuGet v3 endpoints
app.MapNuGetEndpoints();

// Health check endpoint
app.MapHealthChecks("/health");

// Root endpoint with information
app.MapGet("/", () => Results.Json(new
{
    name = "NuGet Server",
    version = "3.0.0",
    description = "A lightweight NuGet v3 protocol server",
    endpoints = new
    {
        serviceIndex = "/v3/index.json",
        health = "/health",
        swagger = "/swagger"
    }
}));

app.Logger.LogInformation("NuGet Server starting on {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Packages directory: {PackagesPath}", packagesPath);

app.Run();
