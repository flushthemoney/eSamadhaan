#!/bin/bash
set -e

echo "Starting eSamadhaan API..."

# Note: Database migrations should be run separately before starting the container
# Use: dotnet ef database update from the backend directory
# Or use a separate migration job/container

# Start the application
echo "Starting application..."
exec dotnet /app/eSamadhaan.API.dll

