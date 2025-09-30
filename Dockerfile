FROM loicsharma/baget:latest
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
ENV Mirror__Enabled=false
ENV Mirror__PackageSource=https://api.nuget.org/v3/index.json
ENV LOG_LEVEL=Information

# Copy custom entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Create directories for packages and database
RUN mkdir -p /var/baget/packages && \
    chmod -R 777 /var/baget

VOLUME ["/var/baget"]

EXPOSE 5000

ENTRYPOINT ["/docker-entrypoint.sh"]
