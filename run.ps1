# Run script for NuGet Gallery Docker container
# Usage: .\run.ps1 [-Tag <tag>] [-Port <port>] [-Name <name>]

param(
    [string]$Tag = "nugetgallery:latest",
    [int]$Port = 8080,
    [string]$Name = "nugetgallery"
)

Write-Host "Starting NuGet Gallery Docker container..." -ForegroundColor Green

# Check if container already exists
$existing = docker ps -a --filter "name=$Name" --format "{{.Names}}"
if ($existing -eq $Name) {
    Write-Host "Container '$Name' already exists." -ForegroundColor Yellow
    $status = docker ps --filter "name=$Name" --format "{{.Status}}"
    if ($status) {
        Write-Host "Container is currently running." -ForegroundColor Yellow
        Write-Host "Access the gallery at: http://localhost:$Port" -ForegroundColor Cyan
        exit 0
    } else {
        Write-Host "Starting existing container..." -ForegroundColor Yellow
        docker start $Name
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Container started successfully!" -ForegroundColor Green
            Write-Host "Access the gallery at: http://localhost:$Port" -ForegroundColor Cyan
        } else {
            Write-Host "Failed to start container." -ForegroundColor Red
            exit 1
        }
        exit 0
    }
}

# Run new container
Write-Host "Running new container..." -ForegroundColor Cyan
docker run -d -p "${Port}:80" --name $Name $Tag

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start container." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Container started successfully!" -ForegroundColor Green
Write-Host "Name: $Name" -ForegroundColor Cyan
Write-Host "URL: http://localhost:$Port" -ForegroundColor Cyan
Write-Host ""
Write-Host "Useful commands:" -ForegroundColor Yellow
Write-Host "  View logs:     docker logs $Name" -ForegroundColor White
Write-Host "  Stop:          docker stop $Name" -ForegroundColor White
Write-Host "  Start:         docker start $Name" -ForegroundColor White
Write-Host "  Remove:        docker rm -f $Name" -ForegroundColor White
Write-Host "  Health check:  docker inspect --format='{{{{json .State.Health}}}}' $Name" -ForegroundColor White
Write-Host ""
Write-Host "Waiting for container to be healthy..." -ForegroundColor Yellow

# Wait for health check
$timeout = 120
$elapsed = 0
$interval = 5

while ($elapsed -lt $timeout) {
    Start-Sleep -Seconds $interval
    $elapsed += $interval
    
    $health = docker inspect --format='{{.State.Health.Status}}' $Name 2>$null
    if ($health -eq "healthy") {
        Write-Host "Container is healthy! ✓" -ForegroundColor Green
        Write-Host "Access the gallery at: http://localhost:$Port" -ForegroundColor Cyan
        exit 0
    }
    
    Write-Host "  Status: $health ($elapsed/$timeout seconds)" -ForegroundColor Gray
}

Write-Host "Warning: Container did not become healthy within timeout period." -ForegroundColor Yellow
Write-Host "The application may still be starting up. Check logs with: docker logs $Name" -ForegroundColor Yellow
