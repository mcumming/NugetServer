#!/bin/bash
# Build script for NuGet.Server Docker container
# This script builds the .NET project and creates a Docker image

set -e  # Exit on error

# Configuration
PROJECT_DIR="NugetServer"
PROJECT_FILE="$PROJECT_DIR/NugetServer.csproj"
IMAGE_NAME="nuget-server"
IMAGE_TAG="latest"
BUILD_CONFIG="Release"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Helper functions
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check prerequisites
log_info "Checking prerequisites..."

if ! command -v dotnet &> /dev/null; then
    log_error "dotnet CLI not found. Please install .NET SDK."
    exit 1
fi

if ! command -v docker &> /dev/null; then
    log_error "docker not found. Please install Docker."
    exit 1
fi

log_info "Prerequisites check passed."

# Step 1: Clean previous build artifacts
log_info "Cleaning previous build artifacts..."
if [ -d "$PROJECT_DIR/bin" ]; then
    rm -rf "$PROJECT_DIR/bin"
fi
if [ -d "$PROJECT_DIR/obj" ]; then
    rm -rf "$PROJECT_DIR/obj"
fi

# Step 2: Restore NuGet packages
log_info "Restoring NuGet packages..."
dotnet restore "$PROJECT_FILE"

# Step 3: Build the project
log_info "Building project in $BUILD_CONFIG configuration..."
dotnet build "$PROJECT_FILE" -c "$BUILD_CONFIG" --no-restore

# Step 4: Verify build output
log_info "Verifying build output..."
BUILD_OUTPUT_DIR="$PROJECT_DIR/bin/$BUILD_CONFIG/net48"
if [ ! -d "$BUILD_OUTPUT_DIR" ]; then
    log_error "Build output directory not found: $BUILD_OUTPUT_DIR"
    exit 1
fi

if [ ! -f "$BUILD_OUTPUT_DIR/NugetServer.dll" ]; then
    log_error "Build output NugetServer.dll not found"
    exit 1
fi

log_info "Build verification successful."

# Step 5: Build Docker image
log_info "Building Docker image: $IMAGE_NAME:$IMAGE_TAG..."
docker build -t "$IMAGE_NAME:$IMAGE_TAG" .

# Step 6: Save Docker image as artifact
log_info "Saving Docker image as build artifact..."
ARTIFACT_DIR="artifacts"
mkdir -p "$ARTIFACT_DIR"
ARTIFACT_FILE="$ARTIFACT_DIR/${IMAGE_NAME}_${IMAGE_TAG}.tar"

docker save -o "$ARTIFACT_FILE" "$IMAGE_NAME:$IMAGE_TAG"

# Compress the artifact
log_info "Compressing artifact..."
gzip -f "$ARTIFACT_FILE"
ARTIFACT_FILE="${ARTIFACT_FILE}.gz"

# Get artifact size
ARTIFACT_SIZE=$(du -h "$ARTIFACT_FILE" | cut -f1)

# Step 7: Summary
log_info "============================================"
log_info "Build completed successfully!"
log_info "============================================"
log_info "Project:      $PROJECT_FILE"
log_info "Build Config: $BUILD_CONFIG"
log_info "Docker Image: $IMAGE_NAME:$IMAGE_TAG"
log_info "Artifact:     $ARTIFACT_FILE"
log_info "Size:         $ARTIFACT_SIZE"
log_info "============================================"
log_info ""
log_info "To load the Docker image, run:"
log_info "  gunzip -c $ARTIFACT_FILE | docker load"
log_info ""
log_info "To run the container, use:"
log_info "  docker run -d -p 8080:8080 --name nuget-server $IMAGE_NAME:$IMAGE_TAG"
log_info ""
log_info "Or use docker-compose:"
log_info "  docker-compose up -d"
