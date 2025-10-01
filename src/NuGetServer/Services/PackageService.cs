using NuGet.Packaging;
using NuGet.Versioning;
using NuGetServer.Configuration;
using NuGetServer.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.IO.Compression;

namespace NuGetServer.Services;

public interface IPackageService
{
    Task<bool> AddPackageAsync(Stream packageStream, CancellationToken cancellationToken = default);
    Task<bool> DeletePackageAsync(string id, string version, CancellationToken cancellationToken = default);
    Task<PackageMetadata?> GetPackageMetadataAsync(string id, string version, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PackageMetadata>> GetPackageVersionsAsync(string id, CancellationToken cancellationToken = default);
    Task<Stream?> GetPackageStreamAsync(string id, string version, CancellationToken cancellationToken = default);
    Task<SearchResult> SearchPackagesAsync(string? query, int skip, int take, bool prerelease, CancellationToken cancellationToken = default);
}

public sealed class FileSystemPackageService(
    IOptions<NuGetServerOptions> options,
    ILogger<FileSystemPackageService> logger) : IPackageService
{
    private readonly NuGetServerOptions _options = options.Value;
    private readonly ILogger<FileSystemPackageService> _logger = logger;
    private readonly ConcurrentDictionary<string, PackageMetadata> _packageCache = new();

    public async Task<bool> AddPackageAsync(Stream packageStream, CancellationToken cancellationToken = default)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await packageStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            using var packageReader = new PackageArchiveReader(memoryStream);
            var nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);
            
            var id = nuspecReader.GetId();
            var version = nuspecReader.GetVersion().ToString();
            
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(version))
            {
                _logger.LogWarning("Package has invalid ID or version");
                return false;
            }

            var packagePath = GetPackagePath(id, version);
            var packageDir = Path.GetDirectoryName(packagePath);
            
            if (!string.IsNullOrEmpty(packageDir))
            {
                Directory.CreateDirectory(packageDir);
            }

            if (File.Exists(packagePath) && !_options.AllowOverwrite)
            {
                _logger.LogWarning("Package {Id} {Version} already exists and overwrite is disabled", id, version);
                return false;
            }

            memoryStream.Position = 0;
            await using (var fileStream = File.Create(packagePath))
            {
                await memoryStream.CopyToAsync(fileStream, cancellationToken);
            }

            var metadata = new PackageMetadata(
                id,
                version,
                nuspecReader.GetDescription(),
                nuspecReader.GetAuthors(),
                nuspecReader.GetTags(),
                DateTime.UtcNow,
                0
            );

            _packageCache[GetCacheKey(id, version)] = metadata;
            _logger.LogInformation("Package {Id} {Version} added successfully", id, version);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding package");
            return false;
        }
    }

    public Task<bool> DeletePackageAsync(string id, string version, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.EnableDelisting)
            {
                _logger.LogWarning("Package deletion is disabled");
                return Task.FromResult(false);
            }

            var packagePath = GetPackagePath(id, version);
            if (!File.Exists(packagePath))
            {
                _logger.LogWarning("Package {Id} {Version} not found", id, version);
                return Task.FromResult(false);
            }

            File.Delete(packagePath);
            _packageCache.TryRemove(GetCacheKey(id, version), out _);
            _logger.LogInformation("Package {Id} {Version} deleted successfully", id, version);
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting package {Id} {Version}", id, version);
            return Task.FromResult(false);
        }
    }

    public async Task<PackageMetadata?> GetPackageMetadataAsync(string id, string version, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(id, version);
        if (_packageCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var packagePath = GetPackagePath(id, version);
        if (!File.Exists(packagePath))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(packagePath);
            using var packageReader = new PackageArchiveReader(stream);
            var nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

            var metadata = new PackageMetadata(
                nuspecReader.GetId(),
                nuspecReader.GetVersion().ToString(),
                nuspecReader.GetDescription(),
                nuspecReader.GetAuthors(),
                nuspecReader.GetTags(),
                File.GetCreationTimeUtc(packagePath),
                0
            );

            _packageCache[cacheKey] = metadata;
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading package metadata for {Id} {Version}", id, version);
            return null;
        }
    }

    public async Task<IReadOnlyList<PackageMetadata>> GetPackageVersionsAsync(string id, CancellationToken cancellationToken = default)
    {
        var packageDir = Path.Combine(_options.PackagesPath, id.ToLowerInvariant());
        if (!Directory.Exists(packageDir))
        {
            return Array.Empty<PackageMetadata>();
        }

        var versions = new List<PackageMetadata>();
        var files = Directory.GetFiles(packageDir, "*.nupkg", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            try
            {
                await using var stream = File.OpenRead(file);
                using var packageReader = new PackageArchiveReader(stream);
                var nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

                var metadata = new PackageMetadata(
                    nuspecReader.GetId(),
                    nuspecReader.GetVersion().ToString(),
                    nuspecReader.GetDescription(),
                    nuspecReader.GetAuthors(),
                    nuspecReader.GetTags(),
                    File.GetCreationTimeUtc(file),
                    0
                );

                versions.Add(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading package from {File}", file);
            }
        }

        return versions.OrderByDescending(v => NuGetVersion.Parse(v.Version)).ToList();
    }

    public Task<Stream?> GetPackageStreamAsync(string id, string version, CancellationToken cancellationToken = default)
    {
        var packagePath = GetPackagePath(id, version);
        if (!File.Exists(packagePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(File.OpenRead(packagePath));
    }

    public async Task<SearchResult> SearchPackagesAsync(
        string? query, 
        int skip, 
        int take, 
        bool prerelease, 
        CancellationToken cancellationToken = default)
    {
        var allPackages = new List<PackageMetadata>();

        if (!Directory.Exists(_options.PackagesPath))
        {
            return new SearchResult(0, Array.Empty<SearchResultItem>());
        }

        var packageDirs = Directory.GetDirectories(_options.PackagesPath);
        
        foreach (var packageDir in packageDirs)
        {
            var files = Directory.GetFiles(packageDir, "*.nupkg", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    await using var stream = File.OpenRead(file);
                    using var packageReader = new PackageArchiveReader(stream);
                    var nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

                    var version = nuspecReader.GetVersion();
                    if (!prerelease && version.IsPrerelease)
                    {
                        continue;
                    }

                    var metadata = new PackageMetadata(
                        nuspecReader.GetId(),
                        version.ToString(),
                        nuspecReader.GetDescription(),
                        nuspecReader.GetAuthors(),
                        nuspecReader.GetTags(),
                        File.GetCreationTimeUtc(file),
                        0
                    );

                    allPackages.Add(metadata);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading package from {File}", file);
                }
            }
        }

        var filtered = allPackages.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.Id.ToLowerInvariant().Contains(lowerQuery) ||
                (p.Description?.ToLowerInvariant().Contains(lowerQuery) ?? false) ||
                (p.Tags?.ToLowerInvariant().Contains(lowerQuery) ?? false)
            );
        }

        var grouped = filtered
            .GroupBy(p => p.Id)
            .Select(g => new SearchResultItem(
                g.Key,
                g.OrderByDescending(v => NuGetVersion.Parse(v.Version)).First().Version,
                g.First().Description,
                g.First().Authors,
                g.First().Tags,
                g.Sum(v => v.DownloadCount),
                g.OrderByDescending(v => NuGetVersion.Parse(v.Version))
                    .Select(v => new SearchResultVersion(v.Version, v.DownloadCount))
                    .ToList()
            ))
            .ToList();

        var totalHits = grouped.Count;
        var results = grouped.Skip(skip).Take(take).ToList();

        return new SearchResult(totalHits, results);
    }

    private string GetPackagePath(string id, string version)
    {
        return Path.Combine(_options.PackagesPath, id.ToLowerInvariant(), version.ToLowerInvariant(), $"{id}.{version}.nupkg");
    }

    private static string GetCacheKey(string id, string version) => $"{id}|{version}";
}
