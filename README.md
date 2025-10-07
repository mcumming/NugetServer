# NuGet Server Docker

A lightweight, containerized NuGet v3 protocol server built with .NET 9, designed to run as a container appliance for hosting private NuGet packages.

## Features

- ✅ Full NuGet v3 protocol implementation
- ✅ Package publishing (push) and deletion
- ✅ Package search and query
- ✅ Package metadata and registration endpoints
- ✅ Built with .NET 9 and modern C# features
- ✅ Multi-stage Dockerfile for optimized image size
- ✅ Comprehensive logging and health checks
- ✅ Configurable via environment variables or configuration files
- ✅ Non-root container user for security
- ✅ File system-based package storage

## Quick Start

### Building the Image

Due to SSL certificate handling in some environments, we provide two Dockerfile options:

#### Option 1: Using the build script (Recommended)

```bash
# Make the script executable (if not already)
chmod +x build.sh

# Run the build script
./build.sh
```

This script will:
1. Build and publish the application locally
2. Create a Docker image using the prebuilt binaries
3. Tag the image as `nuget-server:latest`

#### Option 2: Manual build with Dockerfile.prebuilt

```bash
# Build and publish the application
cd src/NuGetServer
dotnet publish -c Release -o bin/Release/net9.0/publish
cd ../..

# Build Docker image
docker build -f Dockerfile.prebuilt -t nuget-server .
```

#### Option 3: Standard multi-stage Dockerfile

If your environment has proper SSL certificates configured:

```bash
docker build -t nuget-server .
```

**Note:** This may fail in some CI/CD environments due to SSL certificate validation issues with NuGet package restore.

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
# Build the image
docker build -t nuget-server .

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

- .NET 9 SDK
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

The project includes a GitHub Actions CI/CD pipeline that automatically builds, tests, and publishes Docker images.

### Pipeline Features

- **Automated Builds**: Triggers on push to `main` branch
- **Pull Request Validation**: Builds and tests PRs without publishing
- **Container Registry**: Publishes images to GitHub Container Registry (GHCR)
- **Multi-tag Strategy**: Creates tags for branches, versions, and commit SHAs
- **Build Artifacts**: Uploads build artifacts for inspection

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

## Architecture

This implementation follows modern .NET design principles:

- **Minimal API**: Uses .NET 9 minimal APIs for efficient routing
- **Dependency Injection**: Proper DI container usage for services
- **Logging**: Structured logging with configurable log levels
- **Health Checks**: Built-in health check support
- **Configuration**: Flexible configuration using the Options pattern
- **Security**: Runs as non-root user, supports API key authentication

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

