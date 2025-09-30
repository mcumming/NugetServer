#!/bin/bash
# Bash entrypoint script for NuGet Server running on Mono
set -e

echo "Configuring NuGet Server..."

# Update Web.config based on environment variables
if [ -n "$NUGET_API_KEY" ]; then
    echo "API Key: ***configured***"
    # Use xmlstarlet or sed to update Web.config
    sed -i "s|<add key=\"apiKey\" value=\"[^\"]*\"|<add key=\"apiKey\" value=\"$NUGET_API_KEY\"|g" /app/Web.config
else
    echo "API Key: (not set - authentication disabled)"
fi

if [ -n "$NUGET_PACKAGES_PATH" ]; then
    echo "Packages Path: $NUGET_PACKAGES_PATH"
    sed -i "s|<add key=\"packagesPath\" value=\"[^\"]*\"|<add key=\"packagesPath\" value=\"$NUGET_PACKAGES_PATH\"|g" /app/Web.config
    mkdir -p "$NUGET_PACKAGES_PATH"
fi

if [ -n "$NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH" ]; then
    echo "Allow Overwrite: $NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH"
    sed -i "s|<add key=\"allowOverrideExistingPackageOnPush\" value=\"[^\"]*\"|<add key=\"allowOverrideExistingPackageOnPush\" value=\"$NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH\"|g" /app/Web.config
fi

if [ -n "$NUGET_ENABLE_DELISTING" ]; then
    echo "Enable Delisting: $NUGET_ENABLE_DELISTING"
    sed -i "s|<add key=\"enableDelisting\" value=\"[^\"]*\"|<add key=\"enableDelisting\" value=\"$NUGET_ENABLE_DELISTING\"|g" /app/Web.config
fi

echo "NuGet Server configured successfully"
echo "Starting XSP4 web server on port ${ASPNET_PORT:-8080}..."

# Start XSP4 (Mono's ASP.NET web server)
exec xsp4 \
    --root /app \
    --port ${ASPNET_PORT:-8080} \
    --address 0.0.0.0 \
    --nonstop
