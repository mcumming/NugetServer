# Building NuGet.Server Docker Container

This guide explains how to build and run the NuGet.Server Docker container using the official NuGet.Server package with Mono on Linux.

## Prerequisites

### For Building the Application

You need:
- .NET SDK (for restoring and building)
- Docker (for creating container images)

### For Running the Container

- Docker (Linux, macOS, or Windows with WSL2)

## Build Options

### Option 1: CI/CD Pipeline (Recommended for Production)

The repository includes a GitHub Actions workflow that automatically builds and publishes Docker images.

**Workflow Location:** `.github/workflows/build.yml`

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main`
- Manual dispatch via GitHub Actions UI

**What it does:**
1. Restores .NET dependencies
2. Builds the project in Release configuration
3. Verifies build output
4. Creates Docker image with tags `nuget-server:latest` and `nuget-server:<commit-sha>`
5. Compresses and publishes the image as a build artifact

**Using the artifact:**
1. Navigate to the [Actions tab](../../actions) in GitHub
2. Select the latest successful workflow run
3. Download the `nuget-server-image` artifact
4. Load the image:
   ```bash
   gunzip -c nuget-server.tar.gz | docker load
   docker run -d -p 8080:8080 --name nuget-server nuget-server:latest
   ```

**Artifact retention:** Build artifacts are retained for 30 days.

### Option 2: Local Build Script

The easiest way to build locally is to use the automated build script:

```bash
./build.sh
```

This script will:
1. Check prerequisites (dotnet and docker)
2. Clean previous build artifacts
3. Restore NuGet packages
4. Build the project in Release configuration
5. Verify build output
6. Create Docker image
7. Save the image as a compressed artifact in `artifacts/nuget-server_latest.tar.gz`

To load the saved image later:

```bash
gunzip -c artifacts/nuget-server_latest.tar.gz | docker load
```

### Option 3: Manual Build Steps

If you prefer to build manually or need more control:

#### Step 1: Restore NuGet Packages

The project uses SDK-style format with PackageReference:

```bash
cd NugetServer
dotnet restore
```

This will download:
- NuGet.Server 3.4.2
- Dependencies (NuGet.Core, RouteMagic, WebActivatorEx, etc.)

### Step 2: Build the Application

#### Option A: Using dotnet CLI (Recommended)

```bash
dotnet build NugetServer.csproj -c Release
```

#### Option B: Using MSBuild (Command Line)

```bash
# On Windows, locate MSBuild (adjust path for your Visual Studio version)
msbuild NugetServer.csproj /p:Configuration=Release
```

#### Option C: Using Visual Studio

1. Open `NugetServer.csproj` in Visual Studio
2. Select "Release" configuration
3. Build > Build Solution (or press Ctrl+Shift+B)

### Step 3: Verify Build Output

Check that the build output exists:

```bash
ls NugetServer/bin/Release
```
You should see:
- NugetServer.dll
- Web.config
- All NuGet.Server dependencies

### Step 4: Build Docker Image

```bash
cd ..  # Back to repository root
docker build -t nuget-server .
```

### Step 5: Run the Container

```bash
docker run -d -p 8080:8080 \
  -e NUGET_API_KEY=your-secret-key \
  -v nuget-packages:/var/nuget/packages \
  --name nuget-server \
  nuget-server
```

### Step 6: Verify It's Working

```bash
# Test the endpoint
curl http://localhost:8080/nuget

# Or open in browser
# Navigate to http://localhost:8080/nuget
```

## Alternative: Using Docker Compose

```bash
# With environment variables in .env file
docker-compose up -d
```

## Troubleshooting Build Issues

### Missing MSBuild

If MSBuild is not found on Windows:

```powershell
# Find MSBuild
Get-ChildItem "C:\Program Files\Microsoft Visual Studio\" -Recurse -Filter "MSBuild.exe" | Select-Object FullName
```

### NuGet Restore Fails

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Try restore again
dotnet restore
```

### Build Errors

Common issues:
- Missing .NET Framework 4.8: Install from [Microsoft](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- Missing web targets: Install ASP.NET workload in Visual Studio Installer

### Docker Build Fails

Ensure:
- You have build output in `NugetServer/bin/Release/`
- Docker daemon is running
- You have internet connectivity for base image download

## Automated Build Script

Create `build.sh`:

```bash
#!/bin/bash
# Build script for NuGet.Server Docker container

echo "Step 1: Restoring NuGet packages..."
cd NugetServer
dotnet restore

echo "Step 2: Building application..."
dotnet build NugetServer.csproj -c Release

echo "Step 3: Building Docker image..."
cd ..
docker build -t nuget-server .

echo "Build complete!"
echo "Run with: docker run -d -p 8080:8080 --name nuget-server nuget-server"
```

Run it:

```bash
chmod +x build.sh
./build.sh
```

## Continuous Integration

For CI/CD pipelines, you'll need:
- .NET SDK or MSBuild and .NET Framework SDK
- Docker support

Example GitHub Actions workflow:

```yaml
jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v2
        with:
          dotnet-version: '6.0.x'
      - name: Restore NuGet packages
        run: dotnet restore NugetServer/NugetServer.csproj
      - name: Build
        run: dotnet build NugetServer/NugetServer.csproj -c Release
      - name: Build Docker image
        run: docker build -t nuget-server .
```

## Next Steps

After successfully building:
1. Tag your image: `docker tag nuget-server myregistry/nuget-server:latest`
2. Push to registry: `docker push myregistry/nuget-server:latest`
3. Deploy to your environment

See [README.md](README.md) for usage instructions.
