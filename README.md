# NuGet.Server.Docker

A Docker container for the official NuGet.Server package following [Microsoft's NuGet.Server documentation](https://learn.microsoft.com/en-us/nuget/hosting-packages/nuget-server).

## Overview

This project provides a Docker containerized version of the official **NuGet.Server** ASP.NET application running on **Linux containers with Mono**. NuGet.Server is a .NET Framework-based package that creates a NuGet package feed from a simple ASP.NET web application.

### Why Mono?

Mono allows the .NET Framework application to run on Linux containers, providing:
- ✅ Works on Linux, macOS, and Windows
- ✅ Smaller image size (~500MB)
- ✅ More common in Docker ecosystems
- ✅ No need for Windows Server licensing
- ✅ Uses XSP4 (Mono's ASP.NET web server)

## Quick Start

### Prerequisites

- Docker (Linux, macOS, or Windows with WSL2)
- MSBuild or Visual Studio (for building the application)

### Building and Running

```bash
# Build the ASP.NET application (on Windows or with Mono)
cd NugetServer
```bash
# Build the ASP.NET application
cd NugetServer
dotnet restore
dotnet build NugetServer.csproj -c Release

# Build and run with Docker Compose
cd ..
docker-compose up -d
```

The NuGet server will be available at `http://localhost:8080/nuget`

## Building the Application

The NuGet.Server application must be built before creating the Docker image.

### Step 1: Build the ASP.NET Application

The project now uses SDK-style format for easier builds:

```bash
# Navigate to the NugetServer directory
cd NugetServer

# Restore NuGet packages
dotnet restore

# Build the application
dotnet build NugetServer.csproj -c Release
```

Alternatively, you can use MSBuild directly or open `NugetServer.csproj` in Visual Studio and build in Release mode.

### Step 2: Build the Docker Image

```bash
docker build -t nuget-server .
```

## Running the Container

### Linux/Mono Container

#### Basic Usage

```bash
docker run -d -p 8080:8080 --name nuget-server nuget-server:mono
```

The NuGet server will be available at `http://localhost:8080/nuget`

#### With API Key Authentication

```bash
docker run -d -p 8080:8080 \
  -e NUGET_API_KEY=your-secret-api-key \
  --name nuget-server \
  nuget-server:mono
```

#### With Persistent Storage

```bash
docker run -d -p 8080:8080 \
  -v nuget-packages:/var/nuget/packages \
  -e NUGET_API_KEY=your-secret-api-key \
  --name nuget-server \
  nuget-server:mono
```

#### Using Docker Compose

## Running the Container

### Basic Usage

```bash
docker run -d -p 8080:8080 --name nuget-server nuget-server
```

The NuGet server will be available at `http://localhost:8080/nuget`

### With API Key Authentication

```bash
docker run -d -p 8080:8080 \
  -e NUGET_API_KEY=your-secret-api-key \
  --name nuget-server \
  nuget-server
```

### With Persistent Storage

```bash
docker run -d -p 8080:8080 \
  -v nuget-packages:/var/nuget/packages \
  -e NUGET_API_KEY=your-secret-api-key \
  --name nuget-server \
  nuget-server
```

### Using Docker Compose

```bash
docker-compose up -d
```

## Configuration

The container can be configured using environment variables:

| Environment Variable | Description | Default |
|---------------------|-------------|---------|
| `NUGET_API_KEY` | API key for pushing packages. Leave empty to disable authentication | (empty) |
| `NUGET_PACKAGES_PATH` | Path where packages are stored | `/var/nuget/packages` |
| `NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH` | Allow overwriting existing packages | `false` |
| `NUGET_ENABLE_DELISTING` | Enable delisting instead of deletion | `false` |
| `ASPNET_PORT` | Port for the web server | `8080` |

## Using the NuGet Server

### Adding as a Package Source

```bash
dotnet nuget add source http://localhost:8080/nuget -n MyNuGetServer
```

### Publishing Packages

```bash
# With API key
dotnet nuget push MyPackage.1.0.0.nupkg -s http://localhost:8080/nuget -k your-secret-api-key

# Without API key (if authentication is disabled)
dotnet nuget push MyPackage.1.0.0.nupkg -s http://localhost:8080/nuget
```

### Installing Packages

```bash
dotnet add package MyPackage --source MyNuGetServer
```

## Project Structure

```
├── Dockerfile                          # Linux/Mono container Dockerfile
├── docker-compose.yml                  # Docker Compose configuration
├── docker-entrypoint.sh                # Container entrypoint script
├── NugetServer/                        # ASP.NET NuGet.Server application
│   ├── NugetServer.csproj             # SDK-style project file with PackageReference
│   ├── Web.config                     # NuGet.Server configuration
│   └── Properties/
│       └── AssemblyInfo.cs            # Assembly information
└── README.md                          # This file
```

## Features

Based on the official NuGet.Server package (version 3.5.3), this container provides:

- ✅ NuGet v2 and v3 API support
- ✅ Package push/delete operations
- ✅ API key authentication
- ✅ Package overwrite control
- ✅ Package delisting support
- ✅ Configurable package storage
- ✅ File system monitoring for automatic package discovery

## Troubleshooting

- ✅ NuGet v2 and v3 API support
- ✅ Package push/delete operations
- ✅ API key authentication
- ✅ Package overwrite control
- ✅ Package delisting support
- ✅ Configurable package storage
- ✅ File system monitoring for automatic package discovery

## Troubleshooting

### Container won't start

Check the logs:

```bash
docker logs nuget-server
```

Common issues:
- Missing build output in `NugetServer/bin/Release/`
- Permission issues with `/var/nuget/packages` directory

### Cannot push packages

1. Verify the API key is set correctly
2. Check that the packages directory has write permissions
3. Ensure the package doesn't already exist (unless overwrite is enabled)

### Packages not appearing

Wait 15 seconds for the initial cache rebuild, or check the `enableFileSystemMonitoring` setting in Web.config.

### Mono compatibility issues

While Mono supports most .NET Framework features, some ASP.NET features may have limited support. If you encounter issues:
- Check Mono compatibility documentation
- Report issues specific to this project

## Security Considerations

- Always set a strong API key in production
- Use HTTPS in production (configure a reverse proxy)
- Regularly backup the packages volume
- Keep the Mono base image updated

## Related Links

- [Official NuGet.Server Documentation](https://learn.microsoft.com/en-us/nuget/hosting-packages/nuget-server)
- [NuGet.Server Package on NuGet.org](https://www.nuget.org/packages/NuGet.Server/)
- [Mono Project](https://www.mono-project.com/)
- [Mono ASP.NET Support](https://www.mono-project.com/docs/web/aspnet/)

## License

This project is provided as-is under the MIT License. See LICENSE file for details.

