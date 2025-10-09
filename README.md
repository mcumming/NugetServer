# NuGet Server Docker

A lightweight, container#### Advanced Container Configuration

You can customize the container build with additional properties:

```bash
# Build with custom registry and tag
dotnet publish /t:PublishContainer \
  -p ContainerRegistry=ghcr.io \
  -p ContainerRepository=myorg/nuget-server \
  -p ContainerImageTag=v1.0.0

# Build with custom base image
dotnet publish /t:PublishContainer \
  -p ContainerBaseImage=mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Build with custom environment variables
dotnet publish /t:PublishContainer \
  -p ContainerEnvironmentVariable="ASPNETCORE_ENVIRONMENT=Production" \
  -p ContainerEnvironmentVariable="NuGetServer__ApiKey=your-key"

# Build for multiple architectures (if supported by base image)
dotnet publish /t:PublishContainer \
  -p ContainerFamily=jammy-chiseled
```rotocol server built with .NET 8, designed to run as a container appliance for hosting private NuGet packages.

## Features

- ✅ Full NuGet v3 protocol implementation
- ✅ Package publishing (push) and deletion
- ✅ Package search and query
- ✅ Package metadata and registration endpoints
- ✅ Built with .NET 8 and modern C# features
- ✅ Modern .NET SDK container build (no Dockerfiles needed)
- ✅ AOT (Ahead-of-Time) compilation for improved performance
- ✅ Comprehensive logging and health checks
- ✅ Configurable via environment variables or configuration files
- ✅ Non-root container execution with proper permissions
- ✅ File system-based package storage

## Quick Start

### Building the Image

This project uses the .NET SDK's built-in container build feature instead of traditional Dockerfiles for a more streamlined and optimized container creation process.

#### Building with .NET SDK Container Build (Recommended)

```bash
# Make the script executable (if not already)
chmod +x build.sh

# Run the build script
./build.sh
```

This script will use the .NET SDK to build and create a container image directly, tagged as `nuget-server:latest`.

#### Manual build with .NET SDK

```bash
# Navigate to the project directory
cd src/NuGetServer

# Build and create container using .NET SDK
dotnet publish /t:PublishContainer
```

This will create a container image with optimized layers and proper .NET runtime configuration.

#### Advanced Container Configuration

You can customize the container build with additional properties:

```bash
# Build with custom registry and tag
dotnet publish /t:PublishContainer \
  -p ContainerRegistry=ghcr.io \
  -p ContainerRepository=myorg/nuget-server \
  -p ContainerImageTag=v1.0.0

# Build with custom base image
dotnet publish /t:PublishContainer \
  -p ContainerBaseImage=mcr.microsoft.com/dotnet/aspnet:9.0-alpine
```

**Benefits of .NET SDK Container Build:**
- No Dockerfile needed
- Optimized layer caching
- Automatic base image selection
- Built-in security best practices
- Consistent with .NET tooling

> **Note**: This project has been modernized to use the .NET SDK's built-in container build feature. The legacy Dockerfiles have been moved to the `legacy-dockerfiles/` directory for reference.

### Running the Container

### Using Docker Compose

```bash
# Start the server
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the server
docker-compose down
```

The server will be available at `http://localhost:5000`

### Using Docker

```bash
# Build the image using .NET SDK
cd src/NuGetServer
dotnet publish /t:PublishContainer

# Run the container
docker run -d \
  -p 5000:8080 \
  -v nuget-packages:/packages \
  -e NuGetServer__ApiKey=your-api-key-here \
  --name nuget-server \
  nuget-server

# View logs
docker logs -f nuget-server
```

## Configuration

The server can be configured using environment variables or by mounting a custom `appsettings.json` file.

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NuGetServer__PackagesPath` | Directory where packages are stored | `/packages` |
| `NuGetServer__ApiKey` | API key required for push/delete operations (empty = no auth) | `""` |
| `NuGetServer__AllowOverwrite` | Allow overwriting existing packages | `false` |
| `NuGetServer__EnableDelisting` | Allow deleting packages | `true` |
| `NuGetServer__MaxPackageSizeMB` | Maximum package size in MB | `250` |
| `ASPNETCORE_URLS` | URLs the server listens on | `http://+:8080` |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | `Production` |

### Configuration File

You can mount a custom `appsettings.Production.json` file:

```json
{
  "NuGetServer": {
    "PackagesPath": "/packages",
    "ApiKey": "your-secure-api-key",
    "AllowOverwrite": false,
    "EnableDelisting": true,
    "MaxPackageSizeMB": 250
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Mount it in Docker:

```bash
docker run -d \
  -p 5000:8080 \
  -v $(pwd)/appsettings.Production.json:/app/appsettings.Production.json:ro \
  -v nuget-packages:/packages \
  nuget-server
```

## Performance Optimizations

This NuGet server includes several performance optimizations for production use:

### AOT (Ahead-of-Time) Compilation

The container is built with AOT compilation enabled, providing:

- **Faster startup time**: ~2-3x improvement over JIT compilation
- **Lower memory usage**: Reduced memory footprint
- **Smaller container size**: Uses `runtime-deps` base image instead of full ASP.NET runtime
- **Better performance under load**: Pre-compiled native code

### JSON Source Generation

All JSON serialization uses compile-time source generation instead of reflection, ensuring:

- Zero reflection overhead
- Predictable performance
- Full AOT compatibility
- Type-safe serialization

### Container Optimizations

- Minimal base image (`mcr.microsoft.com/dotnet/runtime-deps:8.0`)
- Self-contained deployment with no runtime dependencies
- Optimized for production workloads

## Using with NuGet Client

### Add the source

```bash
# Add the source
dotnet nuget add source http://localhost:5000/v3/index.json \
  --name LocalNuGet

# With API key
dotnet nuget add source http://localhost:5000/v3/index.json \
  --name LocalNuGet \
  --username any \
  --password your-api-key-here \
  --store-password-in-clear-text
```

### Push a package

```bash
dotnet nuget push MyPackage.1.0.0.nupkg \
  --source LocalNuGet \
  --api-key your-api-key-here
```

### Search for packages

```bash
dotnet package search MyPackage --source LocalNuGet
```

### Install a package

```bash
dotnet add package MyPackage --source LocalNuGet
```

### Delete a package

```bash
dotnet nuget delete MyPackage 1.0.0 \
  --source LocalNuGet \
  --api-key your-api-key-here
```

## API Endpoints

The server implements the following NuGet v3 protocol endpoints:

- `GET /v3/index.json` - Service index (entry point)
- `PUT /v3/package` - Push a package
- `DELETE /v3/package/{id}/{version}` - Delete a package
- `GET /v3/package/{id}/{version}/content` - Download package
- `GET /v3/registration/{id}/index.json` - Package registration index
- `GET /v3/registration/{id}/{version}.json` - Package metadata
- `GET /v3/search?q={query}` - Search packages
- `GET /health` - Health check endpoint
- `GET /` - Server information

## Development

### Prerequisites

- .NET 8 SDK
- Docker (optional)

### Build and run locally

```bash
cd src/NuGetServer
dotnet restore
dotnet build
dotnet run
```

The server will start on `http://localhost:5000` (or as configured in `launchSettings.json`).

### Access Swagger UI

When running in Development mode, Swagger UI is available at `http://localhost:5000/swagger`

## CI/CD Pipeline

The project includes a GitHub Actions CI/CD pipeline that automatically builds, tests, and publishes container images using the .NET SDK's built-in container build feature.

### Pipeline Features

- **Automated Builds**: Triggers on push to `main` branch
- **Pull Request Validation**: Builds and tests PRs without publishing
- **Container Registry**: Publishes images to GitHub Container Registry (GHCR)
- **Multi-tag Strategy**: Creates tags for branches, versions, and commit SHAs
- **Build Artifacts**: Uploads build artifacts for inspection
- **.NET SDK Container Build**: Uses modern .NET container build instead of Dockerfiles
- **Optimized Images**: Automatically optimized layers and security best practices

### Image Tags

The pipeline creates multiple tags for published images:

- `latest` - Latest build from the main branch
- `main` - Latest build from the main branch
- `v1.0.0` - Semantic version tags (for tagged releases)
- `sha-abc1234` - Git commit SHA for traceability

### Using Published Images

```bash
# Pull the latest image from GHCR
docker pull ghcr.io/mcumming/nuget.server.docker:latest

# Run the container
docker run -d \
  -p 5000:8080 \
  -v nuget-packages:/packages \
  ghcr.io/mcumming/nuget.server.docker:latest
```

### Triggering Workflows

The CI/CD pipeline can be triggered:

1. **Automatically** - On push to main branch
2. **Pull Requests** - On PR creation/updates to main
3. **Version Tags** - On pushing tags matching `v*.*.*` pattern
4. **Manually** - Via GitHub Actions UI (workflow_dispatch)

The pipeline uses the .NET SDK's container build feature (`dotnet publish /t:PublishContainer`) for creating optimized container images without requiring Dockerfiles.

## Architecture

This implementation follows modern .NET design principles and uses the latest container build technologies:

- **Minimal API**: Uses .NET 8 minimal APIs for efficient routing
- **Dependency Injection**: Proper DI container usage for services
- **Logging**: Structured logging with configurable log levels
- **Health Checks**: Built-in health check support
- **Configuration**: Flexible configuration using the Options pattern
- **Security**: Runs as non-root user, supports API key authentication
- **.NET SDK Container Build**: Uses modern .NET SDK container build instead of Dockerfiles
- **Optimized Images**: Automatic layer optimization and base image selection

### Project Structure

```
src/NuGetServer/
├── Configuration/          # Configuration classes
│   └── NuGetServerOptions.cs
├── Endpoints/             # API endpoint definitions
│   └── NuGetEndpoints.cs
├── Models/                # Data models
│   ├── PackageMetadata.cs
│   └── ServiceIndex.cs
├── Services/              # Business logic
│   └── PackageService.cs
└── Program.cs            # Application entry point
```

## Security Considerations

1. **API Key Protection**: Set a strong API key via environment variable
2. **Network Security**: Use HTTPS in production (configure reverse proxy)
3. **File Permissions**: The container runs as non-root user `nuget`
4. **Package Validation**: Packages are validated using NuGet libraries
5. **Size Limits**: Configurable maximum package size

## Persistence

Packages are stored in the `/packages` directory within the container. Mount a volume to persist packages across container restarts:

```bash
-v /host/path/to/packages:/packages
```

Or use a named volume (recommended):

```bash
-v nuget-packages:/packages
```

### Volume Permissions

The container runs as the non-root `nuget` user (UID typically 999 or 1000 depending on the base image). When using bind mounts, ensure the host directory has appropriate permissions:

```bash
# Create directory with appropriate permissions
mkdir -p /path/to/packages
sudo chown 999:999 /path/to/packages

# Or use chmod if you don't know the exact UID
chmod 777 /path/to/packages  # Less secure but simpler
```

Using named Docker volumes avoids these permission issues as Docker handles the permissions automatically.

## Troubleshooting

### Check server health

```bash
curl http://localhost:5000/health
```

### View logs

```bash
docker logs nuget-server
```

### Verify service index

```bash
curl http://localhost:5000/v3/index.json
```

### Check packages directory

```bash
docker exec nuget-server ls -la /packages
```

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

