using System.Text.Json.Serialization;
using NuGetServer.Models;

namespace NuGetServer;

[JsonSerializable(typeof(ServiceIndex))]
[JsonSerializable(typeof(ServiceResource))]
[JsonSerializable(typeof(ServerInfo))]
[JsonSerializable(typeof(ServerEndpoints))]
[JsonSerializable(typeof(PackageMetadata))]
[JsonSerializable(typeof(SearchResult))]
[JsonSerializable(typeof(SearchResultItem))]
[JsonSerializable(typeof(SearchResultVersion))]
[JsonSerializable(typeof(RegistrationIndex))]
[JsonSerializable(typeof(RegistrationPage))]
[JsonSerializable(typeof(RegistrationLeaf))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class NuGetServerJsonContext : JsonSerializerContext
{
}