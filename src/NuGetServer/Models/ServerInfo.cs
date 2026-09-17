namespace NuGetServer.Models;

public sealed record ServerInfo(
    string Name,
    string Version,
    string Description,
    ServerEndpoints Endpoints
);

public sealed record ServerEndpoints(
    string ServiceIndex,
    string Health,
    string Swagger
);