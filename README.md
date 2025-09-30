# NuGet.Server.Docker

A Docker container for the official NuGet.Server package following [Microsoft's NuGet.Server documentation](https://learn.microsoft.com/en-us/nuget/hosting-packages/nuget-server).

## Overview

This project provides a Docker containerized version of the official **NuGet.Server** ASP.NET application. NuGet.Server is a .NET Framework-based package that creates a NuGet package feed from a simple ASP.NET web application.

## Container Options

This repository provides **two Docker container options**:

### Option 1: Linux Containers with Mono (Recommended)

Uses **Mono** to run the .NET Framework application on Linux containers. This is the recommended approach as:
- ✅ Works on Linux, macOS, and Windows
- ✅ Smaller image size
- ✅ More common in Docker ecosystems
- ✅ No need for Windows Server licensing

**Dockerfile**: `Dockerfile.mono`  
**Compose file**: `docker-compose.mono.yml`

### Option 2: Windows Containers

Uses native Windows containers with IIS. This approach:
- ✅ Native .NET Framework support
- ✅ Full IIS features
- ❌ Requires Windows Server or Windows 10/11 Pro
- ❌ Larger image size
- ❌ Windows containers only

**Dockerfile**: `Dockerfile`  
**Compose file**: `docker-compose.yml`

## Quick Start (Linux/Mono)

### Prerequisites

- Docker (Linux, macOS, or Windows with WSL2)
- MSBuild or Visual Studio (for building the application)

### Building and Running

```bash
# Build the ASP.NET application (on Windows or with Mono)
cd NugetServer
nuget restore
msbuild NugetServer.csproj /p:Configuration=Release

# Build and run with Docker Compose
cd ..
docker-compose -f docker-compose.mono.yml up -d
```

The NuGet server will be available at `http://localhost:8080/nuget`

## Quick Start (Windows Containers)

### Prerequisites

- Docker Desktop with Windows containers enabled
- Windows Server 2022 or Windows 11 (for building)
- Visual Studio 2019 or later with ASP.NET workload (for building the application)
- .NET Framework 4.8 SDK

## Building the Application

The NuGet.Server application must be built before creating the Docker image.

### Step 1: Build the ASP.NET Application

On a Windows machine with Visual Studio installed:

```powershell
# Navigate to the NugetServer directory
cd NugetServer

# Restore NuGet packages
nuget restore

# Build the application
msbuild NugetServer.csproj /p:Configuration=Release
```

Alternatively, open `NugetServer.sln` in Visual Studio and build in Release mode.

### Step 2: Build the Docker Image

#### For Linux/Mono containers:

```bash
docker build -f Dockerfile.mono -t nuget-server:mono .
```

#### For Windows containers:

```powershell
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

```bash
docker-compose -f docker-compose.mono.yml up -d
```

### Windows Container

#### Basic Usage

```powershell
docker run -d -p 8080:80 --name nuget-server nuget-server
```

The NuGet server will be available at `http://localhost:8080/nuget`

#### With API Key Authentication

```powershell
docker run -d -p 8080:80 `
  -e NUGET_API_KEY=your-secret-api-key `
  --name nuget-server `
  nuget-server
```

#### With Persistent Storage

```powershell
docker run -d -p 8080:80 `
  -v nuget-packages:C:\Packages `
  -e NUGET_API_KEY=your-secret-api-key `
  --name nuget-server `
  nuget-server
```

#### Using Docker Compose

```powershell
docker-compose up -d
```

## Configuration

The container can be configured using environment variables:

| Environment Variable | Description | Default (Mono) | Default (Windows) |
|---------------------|-------------|----------------|-------------------|
| `NUGET_API_KEY` | API key for pushing packages. Leave empty to disable authentication | (empty) | (empty) |
| `NUGET_PACKAGES_PATH` | Path where packages are stored | `/var/nuget/packages` | `C:\Packages` |
| `NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH` | Allow overwriting existing packages | `false` | `false` |
| `NUGET_ENABLE_DELISTING` | Enable delisting instead of deletion | `false` | `false` |
| `ASPNET_PORT` | Port for the web server (Mono only) | `8080` | N/A |

## Using the NuGet Server

### Adding as a Package Source

```bash
# For Mono container (port 8080)
dotnet nuget add source http://localhost:8080/nuget -n MyNuGetServer

# For Windows container (port 8080 mapped to 80)
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
├── Dockerfile                          # Windows container Dockerfile
├── Dockerfile.mono                     # Linux/Mono container Dockerfile
├── docker-compose.yml                  # Windows container compose file
├── docker-compose.mono.yml             # Linux/Mono container compose file
├── docker-entrypoint-mono.sh           # Mono entrypoint script
├── NugetServer/                        # ASP.NET NuGet.Server application
│   ├── NugetServer.csproj             # Project file
│   ├── Web.config                     # NuGet.Server configuration
│   ├── packages.config                # NuGet package dependencies
│   ├── docker-entrypoint.ps1          # Container entrypoint script
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

### Container won't start

Ensure you're using Windows containers:

## Troubleshooting

### Container won't start (Windows)

Ensure you're using Windows containers:

```powershell
docker info | Select-String "OS/Arch"
```

Should show `windows/amd64`. If not, switch to Windows containers in Docker Desktop.

### Container won't start (Mono)

Check the logs:

```bash
docker logs nuget-server-mono
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

While Mono supports most .NET Framework features, some ASP.NET features may have limited support. If you encounter issues with the Mono version:
- Try the Windows container version for full compatibility
- Check Mono compatibility documentation
- Report issues specific to this project

## Comparison: Mono vs Windows Containers

| Feature | Mono (Linux) | Windows Container |
|---------|-------------|-------------------|
| Platform support | Linux, macOS, Windows | Windows only |
| Image size | ~500 MB | ~5 GB |
| Startup time | Fast | Slower |
| .NET compatibility | Most features | 100% compatible |
| IIS features | XSP4 only | Full IIS |
| Licensing | Free | Windows Server license may be required |

**Recommendation**: Use Mono containers unless you have specific requirements for Windows containers or need full IIS features.

## Security Considerations

- Always set a strong API key in production
- Use HTTPS in production (configure a reverse proxy)
- Regularly backup the packages volume
- Keep the base images updated (Mono or Windows)

## Related Links

- [Official NuGet.Server Documentation](https://learn.microsoft.com/en-us/nuget/hosting-packages/nuget-server)
- [NuGet.Server Package on NuGet.org](https://www.nuget.org/packages/NuGet.Server/)
- [Mono Project](https://www.mono-project.com/)
- [Mono ASP.NET Support](https://www.mono-project.com/docs/web/aspnet/)
- [Windows Containers Documentation](https://docs.microsoft.com/en-us/virtualization/windowscontainers/)

## License

This project is provided as-is under the MIT License. See LICENSE file for details.


