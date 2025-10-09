# Legacy Dockerfiles

This directory contains the old Dockerfiles that were used before migrating to the .NET SDK container build feature.

These files are kept for reference but are no longer used in the build process.

## Migration to .NET SDK Container Build

As of .NET 8.0.200+, the .NET SDK includes built-in container build support that provides:

- Better performance with optimized layer caching
- Automatic base image selection
- No need for Dockerfiles
- Integration with .NET tooling
- Built-in security best practices

The new build process uses:
```bash
dotnet publish /t:PublishContainer
```

## Files in this directory:

- `Dockerfile` - Original multi-stage Dockerfile
- `Dockerfile.prebuilt` - Dockerfile for prebuilt binaries

These are preserved for historical reference and in case manual Docker builds are needed in specific scenarios.