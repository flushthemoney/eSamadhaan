#!/bin/bash

# Database Seeder Script
# This script seeds the eSamadhaan database with development data

set -e

echo "eSamadhaan Database Seeder"
echo "=========================="
echo ""

# Navigate to seeder project directory
cd "$(dirname "$0")/src/eSamadhaan.DatabaseSeeder"

# Run the seeder
echo "Running database seeder..."
dotnet run

echo ""
echo "Done!"

