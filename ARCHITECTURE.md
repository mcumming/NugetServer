# Architecture and Design

## Overview

This NuGet v3 protocol server is built using modern .NET 9 architecture principles, implementing a lightweight, containerized solution for hosting private NuGet packages.

## Technology Stack

- **.NET 9**: Latest version with performance improvements and modern C# features
- **ASP.NET Core Minimal APIs**: Efficient, lightweight API endpoints
- **NuGet Libraries**: Official NuGet.Protocol and NuGet.Packaging for package handling
- **Docker**: Multi-stage builds for optimized container images
- **Swashbuckle**: OpenAPI/Swagger documentation

## Architecture Principles

### 1. Modern C# and .NET 9 Features

The implementation leverages:
- **Record types**: Immutable data models (`ServiceIndex`, `PackageMetadata`, etc.)
- **Primary constructors**: Simplified constructor syntax in service classes
- **Pattern matching**: Modern C# patterns for cleaner code
- **Nullable reference types**: Explicit null handling for safer code
- **Top-level statements**: Simplified `Program.cs` structure
- **Minimal APIs**: Lightweight endpoint definitions without controllers

### 2. Dependency Injection

All services are registered in the DI container:

```csharp
builder.Services.AddSingleton<IPackageService, FileSystemPackageService>();
```

Services receive dependencies through constructor injection:

```csharp
public sealed class FileSystemPackageService(
    IOptions<NuGetServerOptions> options,
    ILogger<FileSystemPackageService> logger) : IPackageService
```

### 3. Configuration Management

Configuration follows the Options pattern:

- **Strongly-typed**: `NuGetServerOptions` class
- **Multiple sources**: appsettings.json, environment variables, mounted files
- **Hierarchical**: Standard ASP.NET Core configuration hierarchy
- **Validation**: Type-safe configuration access

Environment variables use double-underscore notation:
```bash
NuGetServer__ApiKey=my-key
NuGetServer__PackagesPath=/packages
```

### 4. Logging and Observability

Structured logging throughout:
- **ILogger<T>**: Dependency-injected loggers
- **Structured data**: Proper log parameters for searchability
- **Log levels**: Appropriate levels (Information, Warning, Error)
- **HTTP logging**: Request/response logging in development

### 5. Health Checks

Built-in health check endpoint:
- Used by Docker health checks
- Can be extended with custom health checks
- Useful for orchestration platforms (Kubernetes, Docker Swarm)

## NuGet v3 Protocol Implementation

### Service Index (`/v3/index.json`)

The entry point that advertises all available services:
- SearchQueryService
- RegistrationsBaseUrl
- PackagePublish
- PackageBaseAddress

### Package Operations

#### Push (PUT `/v3/package`)
1. Validates API key (if configured)
2. Reads package stream
3. Extracts metadata using NuGet.Packaging
4. Stores file in organized directory structure
5. Caches metadata in memory

#### Delete (DELETE `/v3/package/{id}/{version}`)
1. Validates API key
2. Checks if delisting is enabled
3. Removes package file
4. Clears cache

#### Download (GET `/v3/package/{id}/{version}/content`)
1. Locates package file
2. Streams file to client
3. Proper content-type headers

### Search and Query

#### Search (GET `/v3/search`)
- Scans package directory
- Filters by query string
- Supports prerelease flag
- Pagination support
- Groups by package ID

#### Registration (GET `/v3/registration/{id}/index.json`)
- Lists all versions of a package
- Provides metadata for each version
- Supports NuGet client version resolution

## Storage Architecture

### File System Layout

```
/packages/
├── packageid1/
│   ├── 1.0.0/
│   │   └── PackageId1.1.0.0.nupkg
│   └── 1.1.0/
│       └── PackageId1.1.1.0.nupkg
└── packageid2/
    └── 2.0.0/
        └── PackageId2.2.0.0.nupkg
```

Benefits:
- Simple, predictable structure
- Easy to backup/restore
- No database required
- Works with standard file system tools

### Caching Strategy

In-memory caching using `ConcurrentDictionary`:
- Cache key: `{packageId}|{version}`
- Reduces disk I/O for frequently accessed metadata
- Thread-safe operations
- Cleared on package deletion

## Security

### 1. API Key Authentication

Optional API key for push/delete operations:
- Header: `X-NuGet-ApiKey`
- Configurable per-operation
- Empty key = no authentication (for private networks)

### 2. Container Security

- **Non-root user**: Runs as `nuget` user
- **Minimal image**: Based on official Microsoft images
- **No unnecessary packages**: Minimal attack surface
- **Health checks**: Automated failure detection

### 3. Input Validation

- Package size limits
- NuGet package format validation
- Path traversal prevention
- Stream handling with proper disposal

## Performance Considerations

### 1. Async/Await Throughout

All I/O operations are asynchronous:
- File operations
- Stream processing
- HTTP responses

### 2. Streaming

Large files are streamed rather than loaded into memory:
- Package uploads
- Package downloads
- Reduces memory pressure

### 3. Efficient Search

- Directory enumeration only when needed
- Metadata caching
- Minimal object allocations

## Extensibility Points

### 1. Storage Backend

`IPackageService` interface allows alternative implementations:
- Azure Blob Storage
- AWS S3
- SQL Server
- MongoDB

### 2. Authentication

Easy to add:
- Bearer token authentication
- OAuth/OIDC
- Certificate-based auth

### 3. Telemetry

Ready for OpenTelemetry integration:
- Activity tracing
- Metrics
- Distributed tracing

## Best Practices Implemented

1. **Separation of Concerns**: Clear layers (Endpoints, Services, Models, Configuration)
2. **SOLID Principles**: Single responsibility, interface-based design
3. **Explicit Dependencies**: Constructor injection, no service locator
4. **Immutability**: Record types, readonly where possible
5. **Async by Default**: All I/O operations are async
6. **Proper Resource Management**: Using statements, IDisposable
7. **Error Handling**: Try-catch where appropriate, structured logging
8. **Configuration Validation**: Options pattern, type safety
9. **API Documentation**: Swagger/OpenAPI support
10. **Container Best Practices**: Multi-stage builds, health checks, non-root user

## Testing Considerations

While this implementation focuses on the core functionality, a production system would include:

1. **Unit Tests**
   - Service layer logic
   - Package validation
   - Configuration handling

2. **Integration Tests**
   - API endpoints
   - File system operations
   - NuGet client compatibility

3. **Performance Tests**
   - Large package handling
   - Concurrent operations
   - Search performance

## Future Enhancements

Potential improvements:
1. Package retention policies
2. Package download statistics
3. Package vulnerability scanning
4. Multi-feed support
5. Symbol server support
6. Package signing verification
7. Bandwidth throttling
8. Database backend option
9. Admin UI
10. Package mirroring/proxying

## References

- [NuGet v3 Protocol Documentation](https://docs.microsoft.com/en-us/nuget/api/overview)
- [ASP.NET Core Minimal APIs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [Docker Multi-stage Builds](https://docs.docker.com/develop/develop-images/multistage-build/)
