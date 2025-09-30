# NuGet.Server.Docker

A lightweight, configurable Docker container for hosting a NuGet server based on [BaGet](https://github.com/loic-sharma/BaGet).

## Quick Start

### Using Docker Compose (Recommended)

```bash
docker-compose up -d
```

The NuGet server will be available at `http://localhost:5000`

### Using Docker Run

```bash
docker build -t nuget-server .
docker run -d -p 5000:5000 -v nuget-data:/var/baget --name nuget-server nuget-server
```

## Configuration

The container can be configured using environment variables. Below are the available options:

### API Key Configuration

Set an API key to secure package publishing:

```yaml
environment:
  - ApiKey=YOUR_SECRET_API_KEY
```

Leave empty or omit for no authentication (development only).

### Storage Configuration

Configure where packages are stored:

```yaml
environment:
  - Storage__Type=FileSystem  # or AzureBlobStorage, AwsS3, GoogleCloud, etc.
  - Storage__Path=/var/baget/packages
```

### Database Configuration

Configure the database backend:

```yaml
environment:
  - Database__Type=Sqlite  # or SqlServer, MySql, PostgreSql
  - Database__ConnectionString=Data Source=/var/baget/baget.db
```

### Mirror Configuration (Read-Through Caching)

Enable mirroring from nuget.org or another NuGet source:

```yaml
environment:
  - Mirror__Enabled=true
  - Mirror__PackageSource=https://api.nuget.org/v3/index.json
```

### Package Deletion Behavior

Configure how package deletions are handled:

```yaml
environment:
  - PackageDeletionBehavior=Unlist  # or HardDelete
```

- `Unlist`: Packages are hidden but not deleted (recommended)
- `HardDelete`: Packages are permanently deleted

### Logging Configuration

Set the logging level:

```yaml
environment:
  - LOG_LEVEL=Information  # or Debug, Warning, Error
```

## Using the NuGet Server

### Publishing Packages

```bash
dotnet nuget push -s http://localhost:5000/v3/index.json -k YOUR_API_KEY package.nupkg
```

### Adding as a Package Source

```bash
dotnet nuget add source http://localhost:5000/v3/index.json -n MyNuGetServer
```

Or add to your `nuget.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="MyNuGetServer" value="http://localhost:5000/v3/index.json" />
  </packageSources>
</configuration>
```

### Restoring Packages

```bash
dotnet restore --source http://localhost:5000/v3/index.json
```

## Volumes

The container uses a volume at `/var/baget` to persist:
- NuGet packages (in `/var/baget/packages`)
- SQLite database (at `/var/baget/baget.db`)

Make sure to mount this volume to preserve your data:

```bash
docker run -v nuget-data:/var/baget ...
```

## Health Check

The container includes a health check endpoint at `/health`. Docker will monitor this endpoint to ensure the service is running properly.

## Advanced Configuration

For more advanced configuration options, refer to the [BaGet documentation](https://loic-sharma.github.io/BaGet/).

### Example: Using with Azure Blob Storage

```yaml
environment:
  - Storage__Type=AzureBlobStorage
  - Storage__AccountName=myaccount
  - Storage__AccessKey=myaccesskey
  - Storage__Container=packages
```

### Example: Using with PostgreSQL

```yaml
environment:
  - Database__Type=PostgreSql
  - Database__ConnectionString=Host=db;Database=baget;Username=user;Password=pass
```

## Production Deployment

For production use, consider:

1. **Set a strong API key** for package publishing
2. **Use a reverse proxy** (nginx, traefik) for HTTPS
3. **Use external storage** (Azure Blob, S3, etc.) for better scalability
4. **Use a production database** (PostgreSQL, SQL Server) instead of SQLite
5. **Enable monitoring and logging**

## Security Notes

- Always set an API key in production environments
- Use HTTPS in production (via reverse proxy)
- Regularly backup the `/var/baget` volume
- Keep the Docker image updated

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

This container is based on [BaGet](https://github.com/loic-sharma/BaGet), an excellent open-source NuGet server implementation.
