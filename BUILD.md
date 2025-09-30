# Building NuGet.Server Docker Container

This guide explains how to build and run the NuGet.Server Docker container using the official NuGet.Server package.

## Prerequisites

### For Building the Application

You need a Windows machine with:
- Visual Studio 2019 or later with ASP.NET and web development workload
- .NET Framework 4.8 SDK
- NuGet CLI tools

### For Running the Container

- Docker Desktop for Windows with Windows containers enabled
- Windows 10/11 Pro or Windows Server 2019/2022

## Build Steps

### Step 1: Install NuGet CLI (if not already installed)

```powershell
# Download NuGet.exe
Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile nuget.exe

# Add to PATH or use full path
```

### Step 2: Restore NuGet Packages

```powershell
cd NugetServer
..\nuget.exe restore packages.config -PackagesDirectory ..\packages
```

This will download:
- NuGet.Server 3.5.3
- Dependencies (NuGet.Core, RouteMagic, WebActivatorEx, etc.)

### Step 3: Build the Application

#### Option A: Using MSBuild (Command Line)

```powershell
# Locate MSBuild (adjust path for your Visual Studio version)
$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

# Build in Release mode
& $msbuild NugetServer.csproj /p:Configuration=Release /p:Platform=AnyCPU
```

#### Option B: Using Visual Studio

1. Open `NugetServer.csproj` in Visual Studio
2. Select "Release" configuration
3. Build > Build Solution (or press Ctrl+Shift+B)

### Step 4: Verify Build Output

Check that the build output exists:

```powershell
dir NugetServer\bin\Release
```

You should see:
- NugetServer.dll
- Web.config
- All NuGet.Server dependencies

### Step 5: Build Docker Image

Switch Docker to Windows containers:

```powershell
# Switch to Windows containers (if not already)
& "C:\Program Files\Docker\Docker\DockerCli.exe" -SwitchDaemon
```

Build the image:

```powershell
cd ..  # Back to repository root
docker build -t nuget-server .
```

### Step 6: Run the Container

```powershell
docker run -d -p 8080:80 `
  -e NUGET_API_KEY=your-secret-key `
  -v nuget-packages:C:\Packages `
  --name nuget-server `
  nuget-server
```

### Step 7: Verify It's Working

```powershell
# Test the endpoint
Invoke-WebRequest -Uri http://localhost:8080/nuget -UseBasicParsing

# Or open in browser
Start-Process http://localhost:8080/nuget
```

## Alternative: Using Docker Compose

```powershell
# With environment variables in .env file
docker-compose up -d
```

## Troubleshooting Build Issues

### Missing MSBuild

If MSBuild is not found:

```powershell
# Find MSBuild
Get-ChildItem "C:\Program Files\Microsoft Visual Studio\" -Recurse -Filter "MSBuild.exe" | Select-Object FullName
```

### NuGet Restore Fails

```powershell
# Clear NuGet cache
nuget.exe locals all -clear

# Try restore again
nuget.exe restore packages.config -PackagesDirectory ..\packages
```

### Build Errors

Common issues:
- Missing .NET Framework 4.8: Install from [Microsoft](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- Missing web targets: Install ASP.NET workload in Visual Studio Installer

### Docker Build Fails

Ensure:
- Docker is set to Windows containers mode
- ServiceMonitor.exe download URL is accessible
- You have internet connectivity for base image download

## Automated Build Script

Create `build.ps1`:

```powershell
# Build script for NuGet.Server Docker container

Write-Host "Step 1: Restoring NuGet packages..." -ForegroundColor Green
.\nuget.exe restore NugetServer\packages.config -PackagesDirectory packages

Write-Host "Step 2: Building application..." -ForegroundColor Green
$msbuild = Get-ChildItem "C:\Program Files\Microsoft Visual Studio\" -Recurse -Filter "MSBuild.exe" | Select-Object -First 1 -ExpandProperty FullName
& $msbuild NugetServer\NugetServer.csproj /p:Configuration=Release /p:Platform=AnyCPU

Write-Host "Step 3: Building Docker image..." -ForegroundColor Green
docker build -t nuget-server .

Write-Host "Build complete!" -ForegroundColor Green
Write-Host "Run with: docker run -d -p 8080:80 --name nuget-server nuget-server"
```

Run it:

```powershell
.\build.ps1
```

## Continuous Integration

For CI/CD pipelines, you'll need:
- Windows-based build agents
- Docker support with Windows containers
- MSBuild and .NET Framework SDK pre-installed

Example GitHub Actions workflow (requires Windows runner):

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
