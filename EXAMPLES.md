# NuGet Server Examples

This document provides practical examples of using the NuGet Server.

## Starting the Server

### Using Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/mcumming/Nuget.Server.Docker.git
cd Nuget.Server.Docker

# Start the server
docker compose up -d

# View logs
docker compose logs -f

# Stop the server
docker compose down
```

### Using Docker Run

```bash
# Build the image
docker build -t nuget-server .

# Run with default settings (no authentication)
docker run -d -p 5000:5000 --name nuget-server nuget-server

# Run with API key authentication
docker run -d -p 5000:5000 \
  -e ApiKey=your-secret-api-key \
  --name nuget-server \
  nuget-server

# Run with persistent storage
docker run -d -p 5000:5000 \
  -v nuget-data:/var/baget \
  -e ApiKey=your-secret-api-key \
  --name nuget-server \
  nuget-server
```

## Publishing Packages

### Configure NuGet Source

Since the server uses HTTP (not HTTPS) in development, you need to configure NuGet to allow insecure connections.

Create or update `nuget.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="LocalNuGet" value="http://localhost:5000/v3/index.json" allowInsecureConnections="true" />
  </packageSources>
</configuration>
```

Or add the source globally:

```bash
dotnet nuget add source http://localhost:5000/v3/index.json \
  -n LocalNuGet \
  --configfile ~/.nuget/NuGet/NuGet.Config
```

### Push a Package

```bash
# Using configured source name
dotnet nuget push MyPackage.1.0.0.nupkg -s LocalNuGet -k your-api-key

# Using URL directly
dotnet nuget push MyPackage.1.0.0.nupkg \
  -s http://localhost:5000/v3/index.json \
  -k your-api-key
```

### Create a Sample Package

```bash
# Create a new class library
dotnet new classlib -n MyLibrary

# Pack it
cd MyLibrary
dotnet pack -c Release -p:PackageVersion=1.0.0

# Push to your NuGet server
dotnet nuget push bin/Release/MyLibrary.1.0.0.nupkg \
  -s http://localhost:5000/v3/index.json \
  -k your-api-key
```

## Consuming Packages

### Restore Packages

Using the configured source in `nuget.config`:

```bash
dotnet restore
```

Or specify the source explicitly:

```bash
dotnet restore --source http://localhost:5000/v3/index.json
```

### Add Package Reference

```bash
# Add package from your NuGet server
dotnet add package MyLibrary --version 1.0.0 --source LocalNuGet
```

Or edit your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="MyLibrary" Version="1.0.0" />
</ItemGroup>
```

## Advanced Configuration

### Enable Mirror Mode (Read-Through Caching)

Mirror mode allows your server to cache packages from NuGet.org:

```yaml
environment:
  - Mirror__Enabled=true
  - Mirror__PackageSource=https://api.nuget.org/v3/index.json
```

With mirror mode enabled, any package request will first check your local server, and if not found, will download from NuGet.org and cache it locally.

### Use PostgreSQL Instead of SQLite

For production deployments, use PostgreSQL:

```yaml
environment:
  - Database__Type=PostgreSql
  - Database__ConnectionString=Host=db;Database=baget;Username=user;Password=pass
```

### Use Azure Blob Storage

For cloud deployments:

```yaml
environment:
  - Storage__Type=AzureBlobStorage
  - Storage__AccountName=myaccount
  - Storage__AccessKey=myaccesskey
  - Storage__Container=packages
```

### Configure with Environment File

Create a `.env` file (see `.env.example`):

```bash
NUGET_API_KEY=your-secret-key
Storage__Type=FileSystem
Database__Type=Sqlite
Mirror__Enabled=true
```

Then use it with docker compose:

```bash
docker compose --env-file .env up -d
```

## Production Deployment

### Using NGINX as Reverse Proxy

Create `nginx.conf`:

```nginx
server {
    listen 443 ssl;
    server_name nuget.example.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;

    location / {
        proxy_pass http://nuget-server:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Increase timeout for large package uploads
        proxy_read_timeout 600s;
        client_max_body_size 200M;
    }
}
```

### Docker Compose with NGINX

```yaml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/conf.d/default.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - nuget-server

  nuget-server:
    build: .
    environment:
      - ApiKey=${NUGET_API_KEY}
    volumes:
      - nuget-data:/var/baget
```

## Troubleshooting

### Container Won't Start

Check logs:

```bash
docker logs nuget-server
```

### Package Upload Fails

1. Verify API key is correct
2. Check that the source allows insecure connections for HTTP
3. Ensure sufficient disk space

### Cannot Find Published Packages

1. Verify the package was pushed successfully
2. Check the search index:
   ```bash
   curl http://localhost:5000/v3/search?q=YourPackage
   ```
3. Verify the database connection

### Performance Issues

1. Consider using PostgreSQL instead of SQLite
2. Use cloud storage (Azure Blob, S3) for better scalability
3. Enable read-through caching if packages are also on NuGet.org

## Web Interface

Access the web interface at `http://localhost:5000` to:
- Browse available packages
- Search for packages
- View package details and dependencies
- Download packages manually

## API Endpoints

- Service Index: `http://localhost:5000/v3/index.json`
- Search: `http://localhost:5000/v3/search?q=query`
- Package Publish: `http://localhost:5000/api/v2/package`
- Package Download: `http://localhost:5000/v3/package/{id}/{version}/{id}.{version}.nupkg`
