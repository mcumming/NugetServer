#!/bin/bash
set -e

# Create appsettings.json from environment variables
cat > /app/appsettings.json << EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "${LOG_LEVEL:-Information}",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "PathBase": "${PATH_BASE:-}",
  "Storage": {
    "Type": "${Storage__Type:-FileSystem}",
    "Path": "${Storage__Path:-/var/baget/packages}"
  },
  "Database": {
    "Type": "${Database__Type:-Sqlite}",
    "ConnectionString": "${Database__ConnectionString:-Data Source=/var/baget/baget.db}"
  },
  "Search": {
    "Type": "${Search__Type:-Database}"
  },
  "Mirror": {
    "Enabled": ${Mirror__Enabled:-false},
    "PackageSource": "${Mirror__PackageSource:-https://api.nuget.org/v3/index.json}"
  },
  "PackageDeletionBehavior": "${PackageDeletionBehavior:-Unlist}",
  "ApiKey": "${ApiKey:-}"
}
EOF

echo "Starting BaGet NuGet Server..."
echo "Configuration:"
echo "  Storage Type: ${Storage__Type:-FileSystem}"
echo "  Storage Path: ${Storage__Path:-/var/baget/packages}"
echo "  Database Type: ${Database__Type:-Sqlite}"
echo "  API Key: ${ApiKey:+***configured***}"
echo "  Mirror Enabled: ${Mirror__Enabled:-false}"

# Start BaGet
exec dotnet BaGet.dll
