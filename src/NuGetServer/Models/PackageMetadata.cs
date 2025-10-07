namespace NuGetServer.Models;

public sealed record PackageMetadata(
    string Id,
    string Version,
    string? Description = null,
    string? Authors = null,
    string? Tags = null,
    DateTime Published = default,
    long DownloadCount = 0
);

public sealed record SearchResult(
    int TotalHits,
    IReadOnlyList<SearchResultItem> Data
);

public sealed record SearchResultItem(
    string Id,
    string Version,
    string? Description,
    string? Authors,
    string? Tags,
    long TotalDownloads,
    IReadOnlyList<SearchResultVersion> Versions
);

public sealed record SearchResultVersion(
    string Version,
    long Downloads
);

public sealed record RegistrationIndex(
    int Count,
    IReadOnlyList<RegistrationPage> Items
);

public sealed record RegistrationPage(
    string Id,
    int Count,
    IReadOnlyList<RegistrationLeaf> Items,
    string? Lower = null,
    string? Upper = null
);

public sealed record RegistrationLeaf(
    string Id,
    string CatalogEntry,
    string PackageContent
);

public sealed record CatalogEntry(
    string Id,
    string Version,
    string? Description,
    string? Authors,
    string? Tags,
    DateTime Published,
    string PackageContent
);
