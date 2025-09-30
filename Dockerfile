# Dockerfile for NuGet.Server on Windows Containers
# This uses the official NuGet.Server package as specified in Microsoft documentation

# Use the official ASP.NET image for Windows containers
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

# Set working directory
WORKDIR /inetpub/wwwroot

# Copy the application files
# Note: The NugetServer application needs to be built on a Windows machine with Visual Studio
# or MSBuild before building this Docker image
COPY NugetServer/bin/Release/ ./

# Copy entrypoint script
COPY NugetServer/docker-entrypoint.ps1 C:/docker-entrypoint.ps1

# Configure environment variables for NuGet.Server
# These can be overridden at runtime
ENV NUGET_API_KEY="" \
    NUGET_PACKAGES_PATH="C:\\Packages" \
    NUGET_ALLOW_OVERWRITE_EXISTING_PACKAGE_ON_PUSH="false" \
    NUGET_ENABLE_DELISTING="false"

# Create packages directory
RUN powershell -Command New-Item -ItemType Directory -Path C:\Packages -Force

# Expose HTTP port
EXPOSE 80

# Use ServiceMonitor to keep container running
# Download and install ServiceMonitor
ADD https://github.com/microsoft/IIS.ServiceMonitor/releases/download/2.0.1.10/ServiceMonitor.exe C:/ServiceMonitor.exe

# Set entrypoint
ENTRYPOINT ["powershell", "-File", "C:\\docker-entrypoint.ps1"]

