# Migration to .NET SDK Container Build

This document outlines the migration from traditional Dockerfiles to the .NET SDK's built-in container build feature.

## Summary of Changes

### 1. Project File Updates (`NuGetServer.csproj`)

Added container build properties:
- `IsPublishable`: true
- `EnableSdkContainerSupport`: true  
- `ContainerBaseImage`: mcr.microsoft.com/dotnet/aspnet:8.0
- `ContainerRepository`: nuget-server
- `ContainerImageTag`: latest
- `ContainerWorkingDirectory`: /app

Added container configuration:
- Environment variables for ASPNETCORE_URLS, ASPNETCORE_ENVIRONMENT, NuGetServer__PackagesPath
- Exposed port 8080

### 2. Build Process Changes

**Before:**
```bash
# Multi-step process with Dockerfile
dotnet publish -c Release -o bin/Release/net8.0/publish
docker build -f Dockerfile.prebuilt -t nuget-server .
```

**After:**
```bash
# Single command with .NET SDK
dotnet publish /t:PublishContainer
```

### 3. Files Removed/Moved

- `Dockerfile` → moved to `legacy-dockerfiles/`
- `Dockerfile.prebuilt` → moved to `legacy-dockerfiles/`
- Removed dependency on `Microsoft.NET.Build.Containers` package (built into SDK 8.0.200+)

### 4. GitHub Actions Workflow Updates

**Before:**
- Used Docker Buildx
- Multi-stage Docker build
- Manual tag management

**After:**
- Uses .NET SDK container build
- Simplified tag management
- Native .NET SDK integration

### 5. Build Script Updates

The `build.sh` script now simply calls:
```bash
dotnet publish /t:PublishContainer
```

### 6. Documentation Updates

- Updated README.md to reflect new build process
- Added advanced configuration examples
- Documented benefits of SDK container build
- Updated version references from .NET 9 to .NET 8

## Benefits of Migration

1. **Simplified Build Process**: Single command builds and containerizes
2. **Better Performance**: Optimized layer caching and base image selection
3. **Security**: Automatic security best practices
4. **Consistency**: Integrated with .NET tooling ecosystem
5. **Maintenance**: No Dockerfile maintenance required

## Backward Compatibility

The legacy Dockerfiles are preserved in `legacy-dockerfiles/` directory for:
- Reference purposes
- Special build scenarios that might require custom Docker steps
- Environments that don't support .NET SDK container builds

## Testing

The migration has been tested and verified:
- ✅ Container builds successfully
- ✅ Container runs and serves NuGet protocol endpoints
- ✅ Health checks work correctly
- ✅ Environment variables are properly configured
- ✅ Port mapping works as expected

## Rollback Plan

If needed, the legacy Dockerfiles can be restored from the `legacy-dockerfiles/` directory and the project can be reverted to the previous build approach by:

1. Moving Dockerfiles back to root directory
2. Reverting project file changes
3. Updating build scripts and CI/CD pipeline
4. Restoring package references if needed