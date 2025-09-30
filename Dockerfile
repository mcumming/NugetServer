FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BAGET_VERSION=1.8.1
WORKDIR /src

# Download BaGet source
RUN apt-get update && apt-get install -y git && \
    git clone --branch v${BAGET_VERSION} --depth 1 https://github.com/loic-sharma/BaGet.git && \
    cd BaGet && \
    dotnet restore src/BaGet/BaGet.csproj

# Build BaGet
WORKDIR /src/BaGet
RUN dotnet build src/BaGet/BaGet.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish src/BaGet/BaGet.csproj -c Release -o /app/publish

FROM base AS final
LABEL maintainer="Michael Cummings (MSFT)"
LABEL org.opencontainers.image.source="https://github.com/mcumming/Nuget.Server.Docker"
LABEL org.opencontainers.image.description="A lightweight NuGet server based on BaGet"

# Environment variables for configuration
ENV ASPNETCORE_URLS=http://+:5000
ENV Storage__Type=FileSystem
ENV Storage__Path=/var/baget/packages
ENV Database__Type=Sqlite
ENV Database__ConnectionString="Data Source=/var/baget/baget.db"
ENV Search__Type=Database
ENV ApiKey=""
ENV PackageDeletionBehavior=Unlist

WORKDIR /app
COPY --from=publish /app/publish .
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Create directories for packages and database
RUN mkdir -p /var/baget/packages && \
    chmod -R 777 /var/baget

VOLUME ["/var/baget"]

ENTRYPOINT ["/docker-entrypoint.sh"]
