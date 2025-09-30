# Building NuGet.Server Docker Container

This guide explains how to build and run the NuGet.Server Docker container using the official NuGet.Server package with Mono on Linux.

## Prerequisites

### For Building the Application

You need:
- Visual Studio 2019 or later with ASP.NET and web development workload (Windows)
- OR MSBuild and .NET Framework 4.8 SDK
- NuGet CLI tools

### For Running the Container

- Docker (Linux, macOS, or Windows with WSL2)

## Build Steps

### Step 1: Install NuGet CLI (if not already installed)

```bash
# Download NuGet.exe
wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
# Or on Windows PowerShell:
# Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile nuget.exe
```

### Step 2: Restore NuGet Packages

```bash
cd NugetServer
nuget restore packages.config -PackagesDirectory ../packages
```

This will download:
- NuGet.Server 3.5.3
- Dependencies (NuGet.Core, RouteMagic, WebActivatorEx, etc.)

### Step 3: Build the Application

#### Option A: Using MSBuild (Command Line)

```bash
# On Windows, locate MSBuild (adjust path for your Visual Studio version)
msbuild NugetServer.csproj /p:Configuration=Release /p:Platform=AnyCPU
```
```

#### Option B: Using Visual Studio

1. Open `NugetServer.csproj` in Visual Studio
2. Select "Release" configuration
3. Build > Build Solution (or press Ctrl+Shift+B)

### Step 4: Verify Build Output

Check that the build output exists:

```bash
ls NugetServer/bin/Release
```

You should see:
- NugetServer.dll
- Web.config
- All NuGet.Server dependencies

### Step 5: Build Docker Image

```bash
cd ..  # Back to repository root
docker build -t nuget-server .
```

### Step 6: Run the Container

```bash
docker run -d -p 8080:8080 \
  -e NUGET_API_KEY=your-secret-key \
  -v nuget-packages:/var/nuget/packages \
  --name nuget-server \
  nuget-server
```

### Step 7: Verify It's Working

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

```powershell
# Clear NuGet cache
nuget.exe locals all -clear

```

### NuGet Restore Fails

```bash
# Clear NuGet cache
nuget locals all -clear

# Try restore again
nuget restore packages.config -PackagesDirectory ../packages
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
nuget restore NugetServer/packages.config -PackagesDirectory packages

echo "Step 2: Building application..."
msbuild NugetServer/NugetServer.csproj /p:Configuration=Release /p:Platform=AnyCPU

echo "Step 3: Building Docker image..."
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
- Build agents with MSBuild and .NET Framework SDK
- Docker support
- NuGet CLI tools

Example GitHub Actions workflow:

```yaml
jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup MSBuild
        uses: microsoft/setup-msbuild@v1
      - name: Restore NuGet packages
        run: nuget restore NugetServer/packages.config -PackagesDirectory packages
      - name: Build
        run: msbuild NugetServer/NugetServer.csproj /p:Configuration=Release
      - name: Build Docker image
        run: docker build -t nuget-server .
```

## Next Steps

After successfully building:
1. Tag your image: `docker tag nuget-server myregistry/nuget-server:latest`
2. Push to registry: `docker push myregistry/nuget-server:latest`
3. Deploy to your environment

See [README.md](README.md) for usage instructions.
