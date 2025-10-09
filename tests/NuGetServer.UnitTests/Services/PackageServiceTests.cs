using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NuGetServer.Configuration;
using NuGetServer.Services;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace NuGetServer.UnitTests.Services;

public class PackageServiceTests : IDisposable
{
    private readonly Mock<ILogger<FileSystemPackageService>> _loggerMock;
    private readonly Mock<IOptions<NuGetServerOptions>> _optionsMock;
    private readonly NuGetServerOptions _options;
    private readonly string _tempDirectory;
    private readonly FileSystemPackageService _packageService;

    public PackageServiceTests()
    {
        _loggerMock = new Mock<ILogger<FileSystemPackageService>>();
        _optionsMock = new Mock<IOptions<NuGetServerOptions>>();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _options = new NuGetServerOptions
        {
            PackagesPath = _tempDirectory,
            AllowOverwrite = false,
            EnableDelisting = true,
            MaxPackageSizeMB = 250
        };

        _optionsMock.Setup(x => x.Value).Returns(_options);
        _packageService = new FileSystemPackageService(_optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddPackageAsync_WithValidPackage_ShouldReturnTrue()
    {
        // Arrange
        var packageStream = CreateTestPackage("TestPackage", "1.0.0");

        // Act
        var result = await _packageService.AddPackageAsync(packageStream);

        // Assert
        result.Should().BeTrue();
        
        // Verify package file exists (PackageService normalizes to lowercase)
        var expectedPath = Path.Combine(_tempDirectory, "testpackage", "1.0.0", "TestPackage.1.0.0.nupkg");
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public async Task AddPackageAsync_WithInvalidPackage_ShouldReturnFalse()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("invalid package content"));

        // Act
        var result = await _packageService.AddPackageAsync(invalidStream);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPackageMetadataAsync_WithExistingPackage_ShouldReturnMetadata()
    {
        // Arrange
        var packageStream = CreateTestPackage("TestPackage", "1.0.0");
        await _packageService.AddPackageAsync(packageStream);

        // Act
        var metadata = await _packageService.GetPackageMetadataAsync("TestPackage", "1.0.0");

        // Assert
        metadata.Should().NotBeNull();
        metadata!.Id.Should().Be("TestPackage");
        metadata.Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task GetPackageMetadataAsync_WithNonExistentPackage_ShouldReturnNull()
    {
        // Act
        var metadata = await _packageService.GetPackageMetadataAsync("NonExistent", "1.0.0");

        // Assert
        metadata.Should().BeNull();
    }

    [Fact]
    public async Task DeletePackageAsync_WithExistingPackage_ShouldReturnTrue()
    {
        // Arrange
        var packageStream = CreateTestPackage("TestPackage", "1.0.0");
        await _packageService.AddPackageAsync(packageStream);

        // Act
        var result = await _packageService.DeletePackageAsync("TestPackage", "1.0.0");

        // Assert
        result.Should().BeTrue();
        
        // Verify package no longer exists
        var metadata = await _packageService.GetPackageMetadataAsync("TestPackage", "1.0.0");
        metadata.Should().BeNull();
    }

    [Fact]
    public async Task DeletePackageAsync_WithNonExistentPackage_ShouldReturnFalse()
    {
        // Act
        var result = await _packageService.DeletePackageAsync("NonExistent", "1.0.0");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPackageVersionsAsync_WithMultipleVersions_ShouldReturnAllVersions()
    {
        // Arrange
        var package1 = CreateTestPackage("TestPackage", "1.0.0");
        var package2 = CreateTestPackage("TestPackage", "2.0.0");
        var package3 = CreateTestPackage("TestPackage", "3.0.0-beta");

        await _packageService.AddPackageAsync(package1);
        await _packageService.AddPackageAsync(package2);
        await _packageService.AddPackageAsync(package3);

        // Act
        var versions = await _packageService.GetPackageVersionsAsync("TestPackage");

        // Assert
        versions.Should().HaveCount(3);
        versions.Select(v => v.Version).Should().Contain(new[] { "1.0.0", "2.0.0", "3.0.0-beta" });
    }

    [Fact]
    public async Task SearchPackagesAsync_WithQuery_ShouldReturnMatchingPackages()
    {
        // Arrange
        var package1 = CreateTestPackage("TestPackage", "1.0.0");
        var package2 = CreateTestPackage("AnotherPackage", "1.0.0");
        var package3 = CreateTestPackage("TestLibrary", "1.0.0");

        await _packageService.AddPackageAsync(package1);
        await _packageService.AddPackageAsync(package2);
        await _packageService.AddPackageAsync(package3);

        // Act
        var result = await _packageService.SearchPackagesAsync("Test", 0, 10, true);

        // Assert
        result.Data.Should().HaveCountGreaterOrEqualTo(2);
        var packageIds = result.Data.Select(p => p.Id).ToList();
        packageIds.Should().Contain("TestPackage");
        packageIds.Should().Contain("TestLibrary");
    }

    [Fact]
    public async Task GetPackageStreamAsync_WithExistingPackage_ShouldReturnStream()
    {
        // Arrange
        var packageStream = CreateTestPackage("TestPackage", "1.0.0");
        await _packageService.AddPackageAsync(packageStream);

        // Act
        var stream = await _packageService.GetPackageStreamAsync("TestPackage", "1.0.0");

        // Assert
        stream.Should().NotBeNull();
        stream!.Length.Should().BeGreaterThan(0);
        stream.Dispose();
    }

    private static MemoryStream CreateTestPackage(string id, string version)
    {
        var memoryStream = new MemoryStream();
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
    <description>Test package description</description>
  </metadata>
</package>";
                var nuspecBytes = Encoding.UTF8.GetBytes(nuspecContent);
                nuspecStream.Write(nuspecBytes, 0, nuspecBytes.Length);
            }

            // Create a dummy content file
            var contentEntry = archive.CreateEntry("lib/net8.0/test.dll");
            using (var contentStream = contentEntry.Open())
            {
                var contentBytes = Encoding.UTF8.GetBytes("dummy content");
                contentStream.Write(contentBytes, 0, contentBytes.Length);
            }
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}