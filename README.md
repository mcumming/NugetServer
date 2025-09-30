# NuGet.Server.Docker

A Docker container for the official NuGet.Server package following [Microsoft's NuGet.Server documentation](https://learn.microsoft.com/en-us/nuget/hosting-packages/nuget-server).

## Overview

This project provides a Docker containerized version of the official **NuGet.Server** ASP.NET application. NuGet.Server is a .NET Framework-based package that creates a NuGet package feed from a simple ASP.NET web application.

## Important Notes

### Windows Containers Required

Since NuGet.Server is built on .NET Framework 4.8, this Docker container **requires Windows containers**. You cannot run this on Linux containers.

### Prerequisites

- Docker Desktop with Windows containers enabled
- Windows Server 2022 or Windows 11 (for building)
- Visual Studio 2019 or later with ASP.NET workload (for building the application)
- .NET Framework 4.8 SDK

## Building the Application

The NuGet.Server application must be built on a Windows machine before creating the Docker image.

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

After building the application:

```powershell
# Build the Docker image
docker build -t nuget-server .
```

## Running the Container

### Basic Usage

```powershell
docker run -d -p 8080:80 --name nuget-server nuget-server
```

The NuGet server will be available at `http://localhost:8080`

### With API Key Authentication

```powershell
docker run -d -p 8080:80 `
  -e NUGET_API_KEY=your-secret-api-key `
  --name nuget-server `
  nuget-server
```

### With Persistent Storage

```powershell
docker run -d -p 8080:80 `
  -v nuget-packages:C:\Packages `
  -e NUGET_API_KEY=your-secret-api-key `
  --name nuget-server `
  nuget-server
```

## Configuration

The container can be configured using environment variables:

| Environment Variable | Description | Default |
|---------------------|-------------|---------|
| `NUGET_API_KEY` | API key for pushing packages. Leave empty to disable authentication | (empty) |
| `NUGET_PACKAGES_PATH` | Path where packages are stored | `C:\Packages` |
| `NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH` | Allow overwriting existing packages | `false` |
| `NUGET_ENABLE_DELISTING` | Enable delisting instead of deletion | `false` |

## Using the NuGet Server

### Adding as a Package Source

```powershell
dotnet nuget add source http://localhost:8080/nuget -n MyNuGetServer
```

### Publishing Packages

```powershell
# With API key
dotnet nuget push MyPackage.1.0.0.nupkg -s http://localhost:8080/nuget -k your-secret-api-key

# Without API key (if authentication is disabled)
dotnet nuget push MyPackage.1.0.0.nupkg -s http://localhost:8080/nuget
```

### Installing Packages

```powershell
dotnet add package MyPackage --source MyNuGetServer
```

## Docker Compose (Alternative)

Create a `docker-compose.yml` file:

```yaml
version: '3.8'

services:
  nuget-server:
    build: .
    image: nuget-server:latest
    ports:
      - "8080:80"
    environment:
      - NUGET_API_KEY=your-secret-api-key
      - NUGET_PACKAGES_PATH=C:\Packages
      - NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH=false
    volumes:
      - nuget-data:C:\Packages

volumes:
  nuget-data:
```

Run with:

```powershell
docker-compose up -d
```

## Project Structure

```
├── Dockerfile                          # Windows container Dockerfile
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

```powershell
docker info | Select-String "OS/Arch"
```

Should show `windows/amd64`. If not, switch to Windows containers in Docker Desktop.

### Cannot push packages

1. Verify the API key is set correctly
2. Check that the packages directory has write permissions
3. Ensure the package doesn't already exist (unless overwrite is enabled)

### Packages not appearing

Wait 15 seconds for the initial cache rebuild, or check the `enableFileSystemMonitoring` setting in Web.config.

## Security Considerations

- Always set a strong API key in production
- Use HTTPS in production (configure a reverse proxy)
- Regularly backup the packages volume
- Keep the Windows container base image updated

## Related Links

- [Official NuGet.Server Documentation](https://learn.microsoft.com/en-us/nuget/hosting-packages/nuget-server)
- [NuGet.Server Package on NuGet.org](https://www.nuget.org/packages/NuGet.Server/)
- [Windows Containers Documentation](https://docs.microsoft.com/en-us/virtualization/windowscontainers/)

## License

This project is provided as-is under the MIT License. See LICENSE file for details.

