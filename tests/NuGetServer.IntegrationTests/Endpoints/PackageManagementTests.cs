using FluentAssertions;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace NuGetServer.IntegrationTests.Endpoints;

public class PackageManagementTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PackageManagementTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PushPackage_WithValidPackage_ShouldSucceed()
    {
        // Arrange
        var packageContent = CreateTestPackage("TestPackage", "1.0.0");
        var form = new MultipartFormDataContent();
        form.Add(new StreamContent(new MemoryStream(packageContent)), "package", "testpackage.1.0.0.nupkg");

        var request = new HttpRequestMessage(HttpMethod.Put, "/v3/package")
        {
            Content = form
        };
        request.Headers.Add("X-NuGet-ApiKey", "test-api-key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PushPackage_WithoutApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var packageContent = CreateTestPackage("TestPackage", "1.0.0");
        var form = new MultipartFormDataContent();
        form.Add(new StreamContent(new MemoryStream(packageContent)), "package", "testpackage.1.0.0.nupkg");

        var request = new HttpRequestMessage(HttpMethod.Put, "/v3/package")
        {
            Content = form
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PushPackage_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var packageContent = CreateTestPackage("TestPackage", "1.0.0");
        var form = new MultipartFormDataContent();
        form.Add(new StreamContent(new MemoryStream(packageContent)), "package", "testpackage.1.0.0.nupkg");

        var request = new HttpRequestMessage(HttpMethod.Put, "/v3/package")
        {
            Content = form
        };
        request.Headers.Add("X-NuGet-ApiKey", "wrong-api-key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PushPackage_WithoutFile_ShouldReturnBadRequest()
    {
        // Arrange - Create empty content (not form data)
        var request = new HttpRequestMessage(HttpMethod.Put, "/v3/package")
        {
            Content = new StringContent("")
        };
        request.Headers.Add("X-NuGet-ApiKey", "test-api-key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("No package file provided");
    }

    [Fact]
    public async Task DownloadPackage_WithExistingPackage_ShouldReturnPackage()
    {
        // Arrange - First push a package
        await PushTestPackage("DownloadTest", "1.0.0");

        // Act
        var response = await _client.GetAsync("/v3/package/DownloadTest/1.0.0/content");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/octet-stream");
        
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DownloadPackage_WithNonExistentPackage_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/v3/package/NonExistent/1.0.0/content");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePackage_WithExistingPackage_ShouldSucceed()
    {
        // Arrange - First push a package
        await PushTestPackage("DeleteTest", "1.0.0");

        var request = new HttpRequestMessage(HttpMethod.Delete, "/v3/package/DeleteTest/1.0.0");
        request.Headers.Add("X-NuGet-ApiKey", "test-api-key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify package is deleted
        var downloadResponse = await _client.GetAsync("/v3/package/DeleteTest/1.0.0/content");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePackage_WithoutApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        await PushTestPackage("DeleteTest2", "1.0.0");

        // Act
        var response = await _client.DeleteAsync("/v3/package/DeleteTest2/1.0.0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePackage_WithNonExistentPackage_ShouldReturnNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/v3/package/NonExistent/1.0.0");
        request.Headers.Add("X-NuGet-ApiKey", "test-api-key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRegistrationIndex_WithExistingPackage_ShouldReturnRegistration()
    {
        // Arrange
        await PushTestPackage("RegistrationTest", "1.0.0");
        await PushTestPackage("RegistrationTest", "2.0.0");

        // Act
        var response = await _client.GetAsync("/v3/registration/RegistrationTest/index.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var registration = JsonSerializer.Deserialize<JsonElement>(content);
        
        var items = registration.GetProperty("items").EnumerateArray();
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRegistrationLeaf_WithExistingPackage_ShouldReturnLeaf()
    {
        // Arrange
        await PushTestPackage("LeafTest", "1.0.0");

        // Act
        var response = await _client.GetAsync("/v3/registration/LeafTest/1.0.0.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var leaf = JsonSerializer.Deserialize<JsonElement>(content);
        
        leaf.GetProperty("id").GetString().Should().Contain("/v3/registration/LeafTest/1.0.0.json");
        leaf.GetProperty("catalogEntry").GetString().Should().Contain("/v3/catalog/LeafTest/1.0.0.json");
        leaf.GetProperty("packageContent").GetString().Should().Contain("/v3/package/LeafTest/1.0.0/content");
    }

    [Fact]
    public async Task SearchPackages_WithQuery_ShouldReturnResults()
    {
        // Arrange
        await PushTestPackage("SearchTest1", "1.0.0");
        await PushTestPackage("SearchTest2", "1.0.0");
        await PushTestPackage("OtherPackage", "1.0.0");

        // Act
        var response = await _client.GetAsync("/v3/search?q=SearchTest&skip=0&take=10&prerelease=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        var data = searchResult.GetProperty("data").EnumerateArray();
        data.Should().HaveCountGreaterOrEqualTo(2);
        
        var packageIds = data.Select(d => d.GetProperty("id").GetString()).ToList();
        packageIds.Should().Contain("SearchTest1");
        packageIds.Should().Contain("SearchTest2");
        packageIds.Should().NotContain("OtherPackage");
    }

    private async Task PushTestPackage(string id, string version)
    {
        var packageContent = CreateTestPackage(id, version);
        var form = new MultipartFormDataContent();
        form.Add(new StreamContent(new MemoryStream(packageContent)), "package", $"{id.ToLowerInvariant()}.{version}.nupkg");

        var request = new HttpRequestMessage(HttpMethod.Put, "/v3/package")
        {
            Content = form
        };
        request.Headers.Add("X-NuGet-ApiKey", "test-api-key");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static byte[] CreateTestPackage(string id, string version)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Create nuspec file
            var nuspecEntry = archive.CreateEntry($"{id}.nuspec");
            using (var nuspecStream = nuspecEntry.Open())
            {
                var nuspecContent = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
  <metadata>
    <id>{id}</id>
    <version>{version}</version>
    <title>{id}</title>
    <authors>Test Author</authors>
    <description>Test package description for {id}</description>
    <tags>test integration</tags>
  </metadata>
</package>";
                var nuspecBytes = Encoding.UTF8.GetBytes(nuspecContent);
                nuspecStream.Write(nuspecBytes, 0, nuspecBytes.Length);
            }

            // Create a dummy content file
            var contentEntry = archive.CreateEntry("lib/net8.0/test.dll");
            using (var contentStream = contentEntry.Open())
            {
                var contentBytes = Encoding.UTF8.GetBytes($"dummy content for {id} {version}");
                contentStream.Write(contentBytes, 0, contentBytes.Length);
            }
        }

        return memoryStream.ToArray();
    }
}