using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGetServer.Configuration;

namespace NuGetServer.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _tempDirectory;

    public TestWebApplicationFactory()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configBuilder =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{NuGetServerOptions.SectionName}:PackagesPath"] = _tempDirectory,
                [$"{NuGetServerOptions.SectionName}:ApiKey"] = "test-api-key",
                [$"{NuGetServerOptions.SectionName}:AllowOverwrite"] = "true",
                [$"{NuGetServerOptions.SectionName}:EnableDelisting"] = "true",
                [$"{NuGetServerOptions.SectionName}:MaxPackageSizeMB"] = "250"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Override logging for tests
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Warning);
            });
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
        base.Dispose(disposing);
    }

    public string TempDirectory => _tempDirectory;
}