#!/bin/sh
set -e

echo "Starting eSamadhaan UI..."

# Note: API URL is set at build time, not runtime
# If you need to change the API URL, rebuild the image with --build-arg API_URL=your-url

# Start nginx
exec "$@"

