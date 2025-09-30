#!/bin/bash
# Quick Start Script for NuGet Server
# This script helps you get started with the NuGet server quickly

set -e

echo "🚀 NuGet Server Quick Start"
echo "============================"
echo ""

# Check if docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Error: Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if docker compose is available
if ! docker compose version &> /dev/null; then
    echo "❌ Error: Docker Compose is not available. Please install Docker Compose."
    exit 1
fi

# Ask for API key
echo "🔐 Configure API Key (optional)"
echo "Leave empty for no authentication (development only)"
read -p "Enter API key (or press Enter to skip): " api_key

# Create .env file if API key is provided
if [ -n "$api_key" ]; then
    cat > .env << EOF
NUGET_API_KEY=$api_key
EOF
    echo "✅ Created .env file with API key"
else
    echo "⚠️  No API key set - authentication is disabled"
fi

echo ""
echo "📦 Building and starting NuGet server..."
docker compose up -d

echo ""
echo "⏳ Waiting for server to be ready..."
sleep 5

# Check if server is running
if curl -s http://localhost:5000/v3/index.json > /dev/null; then
    echo ""
    echo "✅ NuGet Server is running!"
    echo ""
    echo "📍 Access the server at: http://localhost:5000"
    echo "📍 Service Index: http://localhost:5000/v3/index.json"
    echo ""
    
    if [ -n "$api_key" ]; then
        echo "📝 To push packages:"
        echo "   dotnet nuget push MyPackage.nupkg -s http://localhost:5000/v3/index.json -k $api_key"
    else
        echo "📝 To push packages (no API key required):"
        echo "   dotnet nuget push MyPackage.nupkg -s http://localhost:5000/v3/index.json"
    fi
    
    echo ""
    echo "📚 For more examples, see EXAMPLES.md"
    echo ""
    echo "🛑 To stop the server: docker compose down"
else
    echo ""
    echo "❌ Server failed to start. Check logs with: docker compose logs"
    exit 1
fi
