using FluentAssertions;
using NuGetServer.Configuration;
using Xunit;

namespace NuGetServer.UnitTests.Configuration;

public class NuGetServerOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new NuGetServerOptions();

        // Assert
        options.PackagesPath.Should().Be("/packages");
        options.ApiKey.Should().Be(string.Empty);
        options.AllowOverwrite.Should().BeFalse();
        options.EnableDelisting.Should().BeTrue();
        options.MaxPackageSizeMB.Should().Be(250);
        NuGetServerOptions.SectionName.Should().Be("NuGetServer");
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new NuGetServerOptions();

        // Act
        options.PackagesPath = "/custom/path";
        options.ApiKey = "test-api-key";
        options.AllowOverwrite = true;
        options.EnableDelisting = false;
        options.MaxPackageSizeMB = 500;

        // Assert
        options.PackagesPath.Should().Be("/custom/path");
        options.ApiKey.Should().Be("test-api-key");
        options.AllowOverwrite.Should().BeTrue();
        options.EnableDelisting.Should().BeFalse();
        options.MaxPackageSizeMB.Should().Be(500);
    }
}