#!/bin/bash
# Build script for NuGet Server Docker image

set -e

echo "Building NuGet Server..."

# Navigate to source directory
cd "$(dirname "$0")/src/NuGetServer"

# Restore and publish the application
echo "Publishing application..."
dotnet publish -c Release -o bin/Release/net9.0/publish

# Navigate back to root
cd ../..

# Build Docker image using prebuilt binaries
echo "Building Docker image..."
docker build -f Dockerfile.prebuilt -t nuget-server:latest .

echo "Build complete! Docker image: nuget-server:latest"
echo ""
echo "To run the container:"
echo "  docker run -d -p 5000:8080 -v nuget-packages:/packages nuget-server:latest"
echo ""
echo "Or use docker-compose:"
echo "  docker-compose up -d"
