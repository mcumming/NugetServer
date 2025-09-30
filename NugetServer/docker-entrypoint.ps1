# PowerShell entrypoint script for NuGet Server
# This script configures Web.config based on environment variables

$webConfigPath = "C:\inetpub\wwwroot\Web.config"

Write-Host "Configuring NuGet Server..."

# Read the Web.config
[xml]$webConfig = Get-Content $webConfigPath

# Update configuration based on environment variables
if ($env:NUGET_API_KEY) {
    $webConfig.configuration.appSettings.add | Where-Object { $_.key -eq "apiKey" } | ForEach-Object { $_.value = $env:NUGET_API_KEY }
    Write-Host "API Key: ***configured***"
} else {
    Write-Host "API Key: (not set - authentication disabled)"
}

if ($env:NUGET_PACKAGES_PATH) {
    $webConfig.configuration.appSettings.add | Where-Object { $_.key -eq "packagesPath" } | ForEach-Object { $_.value = $env:NUGET_PACKAGES_PATH }
    Write-Host "Packages Path: $env:NUGET_PACKAGES_PATH"
}

if ($env:NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH) {
    $webConfig.configuration.appSettings.add | Where-Object { $_.key -eq "allowOverrideExistingPackageOnPush" } | ForEach-Object { $_.value = $env:NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH }
    Write-Host "Allow Overwrite: $env:NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH"
}

if ($env:NUGET_ENABLE_DELISTING) {
    $webConfig.configuration.appSettings.add | Where-Object { $_.key -eq "enableDelisting" } | ForEach-Object { $_.value = $env:NUGET_ENABLE_DELISTING }
}

# Save the updated Web.config
$webConfig.Save($webConfigPath)

Write-Host "NuGet Server configured successfully"
Write-Host "Starting IIS..."

# Start the IIS service
C:\ServiceMonitor.exe w3svc
