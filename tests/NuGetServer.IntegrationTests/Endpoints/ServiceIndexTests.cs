using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace NuGetServer.IntegrationTests.Endpoints;

public class ServiceIndexTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ServiceIndexTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetServiceIndex_ShouldReturnValidServiceIndex()
    {
        // Act
        var response = await _client.GetAsync("/v3/index.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        var serviceIndex = JsonSerializer.Deserialize<JsonElement>(content);

        serviceIndex.GetProperty("version").GetString().Should().Be("3.0.0");
        
        var resources = serviceIndex.GetProperty("resources").EnumerateArray();
        resources.Should().NotBeEmpty();

        // Verify required resources are present
        var resourceTypes = resources.Select(r => r.GetProperty("type").GetString()).ToList();
        resourceTypes.Should().Contain("SearchQueryService");
        resourceTypes.Should().Contain("RegistrationsBaseUrl");
        resourceTypes.Should().Contain("PackagePublish/2.0.0");
        resourceTypes.Should().Contain("PackageBaseAddress/3.0.0");
    }

    [Fact]
    public async Task GetRootEndpoint_ShouldReturnServerInfo()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        var serverInfo = JsonSerializer.Deserialize<JsonElement>(content);

        serverInfo.GetProperty("name").GetString().Should().Be("NuGet Server");
        serverInfo.GetProperty("version").GetString().Should().Be("3.0.0");
        serverInfo.GetProperty("description").GetString().Should().NotBeNullOrEmpty();
        
        var endpoints = serverInfo.GetProperty("endpoints");
        endpoints.GetProperty("serviceIndex").GetString().Should().Be("/v3/index.json");
        endpoints.GetProperty("health").GetString().Should().Be("/health");
        endpoints.GetProperty("swagger").GetString().Should().Be("/swagger");
    }

    [Fact]
    public async Task GetHealthEndpoint_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }
}