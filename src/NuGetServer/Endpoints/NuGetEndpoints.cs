using Microsoft.AspNetCore.Mvc;
using NuGetServer.Models;
using NuGetServer.Services;
using NuGetServer.Configuration;
using Microsoft.Extensions.Options;

namespace NuGetServer.Endpoints;

public static class NuGetEndpoints
{
    public static IEndpointRouteBuilder MapNuGetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v3");

        // Service Index - the entry point for NuGet v3 protocol
        group.MapGet("/index.json", GetServiceIndex)
            .WithName("GetServiceIndex")
            .WithTags("NuGet");

        // Package Push
        group.MapPut("/package", PushPackage)
            .WithName("PushPackage")
            .WithTags("NuGet")
            .DisableAntiforgery();

        // Package Delete
        group.MapDelete("/package/{id}/{version}", DeletePackage)
            .WithName("DeletePackage")
            .WithTags("NuGet");

        // Package Content (download)
        group.MapGet("/package/{id}/{version}/content", DownloadPackage)
            .WithName("DownloadPackage")
            .WithTags("NuGet");

        // Package Metadata
        group.MapGet("/registration/{id}/index.json", GetRegistrationIndex)
            .WithName("GetRegistrationIndex")
            .WithTags("NuGet");

        group.MapGet("/registration/{id}/{version}.json", GetRegistrationLeaf)
            .WithName("GetRegistrationLeaf")
            .WithTags("NuGet");

        // Search
        group.MapGet("/search", SearchPackages)
            .WithName("SearchPackages")
            .WithTags("NuGet");

        return app;
    }

    private static IResult GetServiceIndex(HttpContext context)
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/v3";

        var serviceIndex = new ServiceIndex(
            "3.0.0",
            new List<ServiceResource>
            {
                new($"{baseUrl}/search", "SearchQueryService", "Query endpoint of NuGet Search service (primary)"),
                new($"{baseUrl}/search", "SearchQueryService/3.0.0-beta", "Query endpoint of NuGet Search service (primary)"),
                new($"{baseUrl}/search", "SearchQueryService/3.0.0-rc", "Query endpoint of NuGet Search service (primary)"),
                new($"{baseUrl}/registration/", "RegistrationsBaseUrl", "Base URL of where package registration info is stored"),
                new($"{baseUrl}/registration/", "RegistrationsBaseUrl/3.0.0-beta", "Base URL of where package registration info is stored"),
                new($"{baseUrl}/registration/", "RegistrationsBaseUrl/3.0.0-rc", "Base URL of where package registration info is stored"),
                new($"{baseUrl}/package", "PackagePublish/2.0.0", "NuGet package push and delete endpoint"),
                new($"{baseUrl}/package/", "PackageBaseAddress/3.0.0", "Base URL of where NuGet packages are stored")
            }
        );

        return Results.Json(serviceIndex, NuGetServerJsonContext.Default.ServiceIndex);
    }

    private static async Task<IResult> PushPackage(
        HttpRequest request,
        IPackageService packageService,
        IOptions<NuGetServerOptions> options,
        ILogger<Program> logger)
    {
        var apiKey = request.Headers["X-NuGet-ApiKey"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(options.Value.ApiKey) && apiKey != options.Value.ApiKey)
        {
            logger.LogWarning("Invalid API key provided");
            return Results.Unauthorized();
        }

        if (!request.HasFormContentType || request.Form.Files.Count == 0)
        {
            return Results.BadRequest("No package file provided");
        }

        var file = request.Form.Files[0];
        
        if (file.Length > options.Value.MaxPackageSizeMB * 1024 * 1024)
        {
            return Results.BadRequest($"Package size exceeds maximum allowed size of {options.Value.MaxPackageSizeMB}MB");
        }

        await using var stream = file.OpenReadStream();
        var success = await packageService.AddPackageAsync(stream);

        if (success)
        {
            logger.LogInformation("Package pushed successfully");
            return Results.Created();
        }

        return Results.BadRequest("Failed to add package");
    }

    private static async Task<IResult> DeletePackage(
        string id,
        string version,
        HttpRequest request,
        IPackageService packageService,
        IOptions<NuGetServerOptions> options,
        ILogger<Program> logger)
    {
        var apiKey = request.Headers["X-NuGet-ApiKey"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(options.Value.ApiKey) && apiKey != options.Value.ApiKey)
        {
            logger.LogWarning("Invalid API key provided");
            return Results.Unauthorized();
        }

        var success = await packageService.DeletePackageAsync(id, version);

        if (success)
        {
            logger.LogInformation("Package {Id} {Version} deleted successfully", id, version);
            return Results.NoContent();
        }

        return Results.NotFound();
    }

    private static async Task<IResult> DownloadPackage(
        string id,
        string version,
        IPackageService packageService)
    {
        var stream = await packageService.GetPackageStreamAsync(id, version);

        if (stream == null)
        {
            return Results.NotFound();
        }

        return Results.Stream(stream, "application/octet-stream", $"{id}.{version}.nupkg");
    }

    private static async Task<IResult> GetRegistrationIndex(
        string id,
        HttpContext context,
        IPackageService packageService)
    {
        var versions = await packageService.GetPackageVersionsAsync(id);

        if (versions.Count == 0)
        {
            return Results.NotFound();
        }

        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/v3";

        var items = versions.Select(v => new RegistrationLeaf(
            $"{baseUrl}/registration/{id}/{v.Version}.json",
            $"{baseUrl}/registration/{id}/{v.Version}.json",
            $"{baseUrl}/package/{id}/{v.Version}/content"
        )).ToList();

        var page = new RegistrationPage(
            $"{baseUrl}/registration/{id}/index.json",
            items.Count,
            items,
            versions.Last().Version,
            versions.First().Version
        );

        var index = new RegistrationIndex(1, new[] { page });

        return Results.Json(index, NuGetServerJsonContext.Default.RegistrationIndex);
    }

    private static async Task<IResult> GetRegistrationLeaf(
        string id,
        string version,
        HttpContext context,
        IPackageService packageService)
    {
        var metadata = await packageService.GetPackageMetadataAsync(id, version);

        if (metadata == null)
        {
            return Results.NotFound();
        }

        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/v3";

        var catalogEntry = new CatalogEntry(
            metadata.Id,
            metadata.Version,
            metadata.Description,
            metadata.Authors,
            metadata.Tags,
            metadata.Published,
            $"{baseUrl}/package/{metadata.Id}/{metadata.Version}/content"
        );

        return Results.Json(catalogEntry, NuGetServerJsonContext.Default.RegistrationLeaf);
    }

    private static async Task<IResult> SearchPackages(
        IPackageService packageService,
        string? q = null,
        int skip = 0,
        int take = 20,
        bool prerelease = true)
    {
        var result = await packageService.SearchPackagesAsync(q, skip, Math.Min(take, 100), prerelease);

        return Results.Json(result, NuGetServerJsonContext.Default.SearchResult);
    }
}
