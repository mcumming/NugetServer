# Multi-stage Dockerfile for NuGet Gallery
# Note: NuGetGallery is built on .NET Framework 4.7.2 (ASP.NET MVC), which requires Windows containers.
# The application cannot run natively on Linux or with .NET Core/.NET 9 without significant rewriting.

# Build stage
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2022 AS build

# Set working directory
WORKDIR /src

# Install Git to clone the repository
RUN powershell -Command \
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; \
    Invoke-WebRequest -Uri 'https://github.com/git-for-windows/git/releases/download/v2.42.0.windows.2/MinGit-2.42.0.2-64-bit.zip' -OutFile git.zip; \
    Expand-Archive -Path git.zip -DestinationPath C:\git; \
    Remove-Item git.zip

# Add git to PATH
RUN setx /M PATH "%PATH%;C:\git\cmd"

# Clone the NuGetGallery repository
RUN git clone --depth 1 https://github.com/nuget/nugetgallery.git C:\nugetgallery

# Set working directory to the cloned repository
WORKDIR /nugetgallery

# Restore NuGet packages
RUN nuget restore NuGetGallery.sln

# Build the solution to ensure all dependencies are built
RUN msbuild NuGetGallery.sln /p:Configuration=Release /p:Platform="Any CPU" /m

# Publish the NuGetGallery web project
RUN msbuild src\NuGetGallery\NuGetGallery.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:DeployOnBuild=true /p:WebPublishMethod=FileSystem /p:publishUrl=C:\publish /p:DeleteExistingFiles=False /t:WebPublish

# Runtime stage
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022 AS runtime

# Set working directory
WORKDIR /inetpub/wwwroot

# Copy published application from build stage
COPY --from=build /publish .

# Expose port 80
EXPOSE 80

# Configure IIS
RUN powershell -Command \
    Import-Module WebAdministration; \
    Remove-Website -Name 'Default Web Site'; \
    New-Website -Name 'NuGetGallery' -PhysicalPath 'C:\inetpub\wwwroot' -Port 80 -Force

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD powershell -Command "try { (Invoke-WebRequest -Uri http://localhost -UseBasicParsing).StatusCode -eq 200 } catch { exit 1 }"

# Start IIS
ENTRYPOINT ["C:\\ServiceMonitor.exe", "w3svc"]
