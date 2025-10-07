# NuGet Server Examples

## Example 1: Basic Usage with Named Volume

```bash
# Build the image
./build.sh

# Start the server using docker-compose
docker-compose up -d

# Check if it's running
curl http://localhost:5000/health

# View the service index
curl http://localhost:5000/v3/index.json
```

## Example 2: Testing with NuGet CLI

```bash
# Add the source
dotnet nuget add source http://localhost:5000/v3/index.json \
  --name LocalNuGet

# Create a test package
mkdir TestPackage && cd TestPackage
cat > TestPackage.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>MyTestPackage</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>A test package</Description>
  </PropertyGroup>
</Project>
EOF

# Pack the package
dotnet pack -o .

# Push to your NuGet server (if no API key is set)
curl -X PUT http://localhost:5000/v3/package \
  -F "package=@MyTestPackage.1.0.0.nupkg"

# Or use dotnet CLI with API key
dotnet nuget push MyTestPackage.1.0.0.nupkg \
  --source LocalNuGet \
  --api-key your-api-key

# Search for packages
curl "http://localhost:5000/v3/search?q=test"

# Install the package in another project
dotnet add package MyTestPackage --source LocalNuGet
```

## Example 3: Running with Custom Configuration

```bash
# Create a custom configuration file
cat > appsettings.Production.json << 'EOF'
{
  "NuGetServer": {
    "PackagesPath": "/packages",
    "ApiKey": "my-secure-api-key-12345",
    "AllowOverwrite": false,
    "EnableDelisting": true,
    "MaxPackageSizeMB": 500
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
EOF

# Run with custom config
docker run -d \
  -p 5000:8080 \
  -v $(pwd)/appsettings.Production.json:/app/appsettings.Production.json:ro \
  -v nuget-packages:/packages \
  --name nuget-server \
  nuget-server:latest
```

## Example 4: Behind a Reverse Proxy (nginx)

```nginx
server {
    listen 80;
    server_name nuget.example.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Increase timeouts for large package uploads
        proxy_connect_timeout 300s;
        proxy_send_timeout 300s;
        proxy_read_timeout 300s;
        
        # Increase max body size for package uploads
        client_max_body_size 500M;
    }
}
```

## Example 5: Kubernetes Deployment

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: nuget-packages
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 10Gi
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nuget-server
spec:
  replicas: 1
  selector:
    matchLabels:
      app: nuget-server
  template:
    metadata:
      labels:
        app: nuget-server
    spec:
      containers:
      - name: nuget-server
        image: nuget-server:latest
        ports:
        - containerPort: 8080
        env:
        - name: NuGetServer__ApiKey
          valueFrom:
            secretKeyRef:
              name: nuget-secrets
              key: api-key
        - name: NuGetServer__PackagesPath
          value: "/packages"
        volumeMounts:
        - name: packages
          mountPath: /packages
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 10
      volumes:
      - name: packages
        persistentVolumeClaim:
          claimName: nuget-packages
---
apiVersion: v1
kind: Service
metadata:
  name: nuget-server
spec:
  selector:
    app: nuget-server
  ports:
  - port: 80
    targetPort: 8080
  type: LoadBalancer
```
