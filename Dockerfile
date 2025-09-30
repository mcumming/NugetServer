# Alternative Dockerfile using Mono to run NuGet.Server on Linux containers
# This eliminates the need for Windows containers

FROM mono:latest

# Install dependencies
RUN apt-get update && \
    apt-get install -y \
    ca-certificates \
    wget \
    unzip \
    && rm -rf /var/lib/apt/lists/*

# Install XSP (Mono's ASP.NET web server)
RUN apt-get update && \
    apt-get install -y mono-xsp4 && \
    rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Copy the application files
# Note: The NugetServer application needs to be built before building this Docker image
COPY NugetServer/bin/Release/ ./

# Copy entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Configure environment variables for NuGet.Server
ENV NUGET_API_KEY="" \
    NUGET_PACKAGES_PATH="/var/nuget/packages" \
    NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH="false" \
    NUGET_ENABLE_DELISTING="false" \
    ASPNET_PORT="8080"

# Create packages directory
RUN mkdir -p /var/nuget/packages && \
    chmod 777 /var/nuget/packages

# Expose HTTP port
EXPOSE 8080

# Set entrypoint
ENTRYPOINT ["/docker-entrypoint.sh"]
