# Build script for NuGet Gallery Docker image
# Usage: .\build.ps1 [-Tag <tag>] [-NoPull]

param(
    [string]$Tag = "nugetgallery:latest",
    [switch]$NoPull
)

Write-Host "Building NuGet Gallery Docker image..." -ForegroundColor Green
Write-Host "Tag: $Tag" -ForegroundColor Cyan

$startTime = Get-Date

# Ensure we're in Windows container mode
$dockerInfo = docker info 2>&1 | Out-String
if ($dockerInfo -notmatch "OSType: windows") {
    Write-Host "ERROR: Docker is not running in Windows container mode." -ForegroundColor Red
    Write-Host "Please switch to Windows containers using the Docker Desktop system tray icon." -ForegroundColor Yellow
    exit 1
}

# Build arguments
$buildArgs = @(
    "build",
    "-t", $Tag,
    "."
)

if (-not $NoPull) {
    $buildArgs += "--pull"
}

Write-Host "Running: docker $($buildArgs -join ' ')" -ForegroundColor Cyan
& docker $buildArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Duration: $($duration.ToString('hh\:mm\:ss'))" -ForegroundColor Cyan
Write-Host "Image: $Tag" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the container:" -ForegroundColor Yellow
Write-Host "  docker run -d -p 8080:80 --name nugetgallery $Tag" -ForegroundColor White
Write-Host ""
Write-Host "To view logs:" -ForegroundColor Yellow
Write-Host "  docker logs nugetgallery" -ForegroundColor White
