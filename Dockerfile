# Multi-stage Dockerfile for NuGet Server
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Install ca-certificates for SSL
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates

WORKDIR /src

# Copy project file and restore dependencies
COPY ["src/NuGetServer/NuGetServer.csproj", "NuGetServer/"]
RUN dotnet restore "NuGetServer/NuGetServer.csproj"

# Copy source code and build
COPY src/NuGetServer/ NuGetServer/
WORKDIR "/src/NuGetServer"
RUN dotnet build "NuGetServer.csproj" -c Release -o /app/build --no-restore

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "NuGetServer.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r nuget && useradd -r -g nuget nuget

# Create packages directory
RUN mkdir -p /packages && chown nuget:nuget /packages

# Copy published application
COPY --from=publish /app/publish .

# Switch to non-root user
USER nuget

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV NuGetServer__PackagesPath=/packages

# Entry point
ENTRYPOINT ["dotnet", "NuGetServer.dll"]
