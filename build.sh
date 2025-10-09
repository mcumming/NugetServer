#!/bin/bash
# Build script for NuGet Server using .NET SDK container build

set -e

echo "Building NuGet Server container using .NET SDK..."

# Navigate to source directory
cd "$(dirname "$0")/src/NuGetServer"

# Build and publish as container using .NET SDK container build with AOT
echo "Building container with .NET SDK and AOT compilation..."
dotnet publish /t:PublishContainer -c Release

echo "Build complete! Docker image: nuget-server:latest"
echo ""
echo "To run the container:"
echo "  docker run -d -p 5000:8080 -v nuget-packages:/packages nuget-server:latest"
echo ""
echo "Or use docker-compose:"
echo "  docker-compose up -d"
