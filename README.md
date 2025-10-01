# Nuget.Server.Docker

A Docker container for the NuGet Gallery - the official NuGet package hosting service that powers nuget.org.

## Overview

This repository provides a Docker container build for the [NuGet Gallery](https://github.com/nuget/nugetgallery) project. The container image is built using a multi-stage Dockerfile that:

1. Checks out the latest source from https://github.com/nuget/nugetgallery
2. Builds the NuGetGallery project
3. Creates a runtime container image with the compiled binaries

## Technical Notes

**Important:** The NuGet Gallery is built on .NET Framework 4.7.2 (ASP.NET MVC), which requires Windows containers. The application cannot run natively on Linux or with .NET Core/.NET 9 without significant architectural changes to the upstream project.

This implementation uses:
- **Build Stage:** `mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2022`
- **Runtime Stage:** `mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022`
- **Web Server:** Internet Information Services (IIS)

## Prerequisites

- Docker Desktop for Windows with Windows containers enabled
- Windows Server 2022 or Windows 10/11 with Hyper-V
- At least 10GB of free disk space for the image build

## Building the Container

To build the Docker image:

```powershell
docker build -t nugetgallery:latest .
```

The build process will:
1. Clone the NuGet Gallery repository
2. Restore NuGet packages
3. Build the project using MSBuild
4. Publish the web application
5. Create a runtime container with IIS configured

**Note:** The initial build may take 15-30 minutes depending on your system and network speed.

## Running the Container

To run the NuGet Gallery container:

```powershell
docker run -d -p 8080:80 --name nugetgallery nugetgallery:latest
```

Access the gallery at: http://localhost:8080

## Configuration

The NuGet Gallery requires several configuration settings for full functionality:

- Database connection strings
- Azure Storage account (for package storage)
- SMTP settings (for email notifications)
- Application Insights (for telemetry)

For production deployment, you'll need to:

1. Configure environment variables or mount a configuration file
2. Set up a SQL Server database
3. Configure Azure Storage or alternative package storage
4. Review the [NuGet Gallery deployment documentation](https://github.com/NuGet/NuGetGallery/tree/main/docs/Deploying)

## Container Management

### Stop the container
```powershell
docker stop nugetgallery
```

### Start the container
```powershell
docker start nugetgallery
```

### View logs
```powershell
docker logs nugetgallery
```

### Remove the container
```powershell
docker rm -f nugetgallery
```

## Health Check

The container includes a health check that verifies the web server is responding on port 80. Check health status:

```powershell
docker inspect --format='{{json .State.Health}}' nugetgallery
```

## Development

To modify the build process:

1. Edit the `Dockerfile` to customize build parameters
2. Add environment-specific configuration files
3. Rebuild the image

## Troubleshooting

### Build fails with "nuget command not found"
The SDK image includes nuget.exe. If you encounter this error, verify you're using the correct base image.

### Container fails to start
Check the container logs for specific errors:
```powershell
docker logs nugetgallery
```

### Cannot access the gallery
1. Verify the container is running: `docker ps`
2. Check the port mapping: `docker port nugetgallery`
3. Ensure Windows Firewall allows the connection
4. Try accessing from the container host first

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

MIT License - See [LICENSE](LICENSE) file for details.

The NuGet Gallery source code is licensed under Apache 2.0. See the [upstream repository](https://github.com/nuget/nugetgallery) for details.

## References

- [NuGet Gallery GitHub Repository](https://github.com/nuget/nugetgallery)
- [NuGet Documentation](https://docs.microsoft.com/nuget/)
- [Docker Windows Containers](https://docs.microsoft.com/virtualization/windowscontainers/)
