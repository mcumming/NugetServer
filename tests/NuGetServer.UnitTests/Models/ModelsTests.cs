using FluentAssertions;
using NuGetServer.Models;
using Xunit;

namespace NuGetServer.UnitTests.Models;

public class PackageMetadataTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = "TestPackage";
        var version = "1.0.0";
        var description = "Test description";
        var authors = "Author1, Author2";
        var tags = "tag1 tag2";
        var published = DateTime.UtcNow;
        var downloadCount = 100L;

        // Act
        var metadata = new PackageMetadata(id, version, description, authors, tags, published, downloadCount);

        // Assert
        metadata.Id.Should().Be(id);
        metadata.Version.Should().Be(version);
        metadata.Description.Should().Be(description);
        metadata.Authors.Should().Be(authors);
        metadata.Tags.Should().Be(tags);
        metadata.Published.Should().Be(published);
        metadata.DownloadCount.Should().Be(downloadCount);
    }

    [Fact]
    public void Constructor_WithDefaults_ShouldHandleGracefully()
    {
        // Act
        var metadata = new PackageMetadata("TestPackage", "1.0.0");

        // Assert
        metadata.Id.Should().Be("TestPackage");
        metadata.Version.Should().Be("1.0.0");
        metadata.Description.Should().BeNull();
        metadata.Authors.Should().BeNull();
        metadata.Tags.Should().BeNull();
        metadata.Published.Should().Be(default);
        metadata.DownloadCount.Should().Be(0);
    }
}

public class SearchResultTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new SearchResultItem("Package1", "1.0.0", "Desc1", "Author1", "tag1", 100, 
                new[] { new SearchResultVersion("1.0.0", 100) }),
            new SearchResultItem("Package2", "2.0.0", "Desc2", "Author2", "tag2", 200, 
                new[] { new SearchResultVersion("2.0.0", 200) })
        };
        var totalHits = 10;

        // Act
        var result = new SearchResult(totalHits, data);

        // Assert
        result.Data.Should().BeEquivalentTo(data);
        result.TotalHits.Should().Be(totalHits);
    }
}

public class SearchResultItemTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = "TestPackage";
        var version = "1.0.0";
        var description = "Test description";
        var authors = "Test Author";
        var tags = "test";
        var totalDownloads = 1000L;
        var versions = new[] { new SearchResultVersion("1.0.0", 500), new SearchResultVersion("2.0.0", 500) };

        // Act
        var item = new SearchResultItem(id, version, description, authors, tags, totalDownloads, versions);

        // Assert
        item.Id.Should().Be(id);
        item.Version.Should().Be(version);
        item.Description.Should().Be(description);
        item.Authors.Should().Be(authors);
        item.Tags.Should().Be(tags);
        item.TotalDownloads.Should().Be(totalDownloads);
        item.Versions.Should().BeEquivalentTo(versions);
    }
}

public class SearchResultVersionTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var version = "1.0.0";
        var downloads = 1000L;

        // Act
        var resultVersion = new SearchResultVersion(version, downloads);

        // Assert
        resultVersion.Version.Should().Be(version);
        resultVersion.Downloads.Should().Be(downloads);
    }
}

public class RegistrationIndexTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new RegistrationPage("page1", 1, new[] { new RegistrationLeaf("leaf1", "catalog1", "content1") }),
            new RegistrationPage("page2", 1, new[] { new RegistrationLeaf("leaf2", "catalog2", "content2") })
        };
        var count = 2;

        // Act
        var index = new RegistrationIndex(count, items);

        // Assert
        index.Count.Should().Be(count);
        index.Items.Should().BeEquivalentTo(items);
    }
}

public class RegistrationPageTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = "page1";
        var count = 1;
        var items = new[] { new RegistrationLeaf("leaf1", "catalog1", "content1") };
        var lower = "1.0.0";
        var upper = "2.0.0";

        // Act
        var page = new RegistrationPage(id, count, items, lower, upper);

        // Assert
        page.Id.Should().Be(id);
        page.Count.Should().Be(count);
        page.Items.Should().BeEquivalentTo(items);
        page.Lower.Should().Be(lower);
        page.Upper.Should().Be(upper);
    }
}

public class RegistrationLeafTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = "leaf1";
        var catalogEntry = "catalog1";
        var packageContent = "content1";

        // Act
        var leaf = new RegistrationLeaf(id, catalogEntry, packageContent);

        // Assert
        leaf.Id.Should().Be(id);
        leaf.CatalogEntry.Should().Be(catalogEntry);
        leaf.PackageContent.Should().Be(packageContent);
    }
}

public class CatalogEntryTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = "TestPackage";
        var version = "1.0.0";
        var description = "Test description";
        var authors = "Test Author";
        var tags = "test";
        var published = DateTime.UtcNow;
        var packageContent = "content";

        // Act
        var entry = new CatalogEntry(id, version, description, authors, tags, published, packageContent);

        // Assert
        entry.Id.Should().Be(id);
        entry.Version.Should().Be(version);
        entry.Description.Should().Be(description);
        entry.Authors.Should().Be(authors);
        entry.Tags.Should().Be(tags);
        entry.Published.Should().Be(published);
        entry.PackageContent.Should().Be(packageContent);
    }
}

public class ServiceIndexTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var version = "3.0.0";
        var resources = new[]
        {
            new ServiceResource("http://example.com/search", "SearchQueryService", "Search service"),
            new ServiceResource("http://example.com/registration/", "RegistrationsBaseUrl", "Registration service")
        };

        // Act
        var serviceIndex = new ServiceIndex(version, resources);

        // Assert
        serviceIndex.Version.Should().Be(version);
        serviceIndex.Resources.Should().BeEquivalentTo(resources);
    }
}

public class ServiceResourceTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = "http://example.com/search";
        var type = "SearchQueryService";
        var comment = "Search service endpoint";

        // Act
        var resource = new ServiceResource(id, type, comment);

        // Assert
        resource.Id.Should().Be(id);
        resource.Type.Should().Be(type);
        resource.Comment.Should().Be(comment);
    }
}

public class ServerInfoTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var name = "NuGet Server";
        var version = "1.0.0";
        var description = "A NuGet server";
        var endpoints = new ServerEndpoints("/v3/index.json", "/health", "/swagger");

        // Act
        var serverInfo = new ServerInfo(name, version, description, endpoints);

        // Assert
        serverInfo.Name.Should().Be(name);
        serverInfo.Version.Should().Be(version);
        serverInfo.Description.Should().Be(description);
        serverInfo.Endpoints.Should().Be(endpoints);
    }
}

public class ServerEndpointsTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var serviceIndex = "/v3/index.json";
        var health = "/health";
        var swagger = "/swagger";

        // Act
        var endpoints = new ServerEndpoints(serviceIndex, health, swagger);

        // Assert
        endpoints.ServiceIndex.Should().Be(serviceIndex);
        endpoints.Health.Should().Be(health);
        endpoints.Swagger.Should().Be(swagger);
    }
}