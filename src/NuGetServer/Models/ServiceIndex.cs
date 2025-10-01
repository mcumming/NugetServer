namespace NuGetServer.Models;

public sealed record ServiceIndex(
    string Version,
    IReadOnlyList<ServiceResource> Resources
);

public sealed record ServiceResource(
    string Id,
    string Type,
    string? Comment = null
);
