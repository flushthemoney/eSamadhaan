# Database Seeder Script (PowerShell)
# This script seeds the eSamadhaan database with development data

Write-Host "eSamadhaan Database Seeder" -ForegroundColor Cyan
Write-Host "==========================" -ForegroundColor Cyan
Write-Host ""

# Navigate to seeder project directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$seederPath = Join-Path $scriptPath "src\eSamadhaan.DatabaseSeeder"

Set-Location $seederPath

# Run the seeder
Write-Host "Running database seeder..." -ForegroundColor Yellow
dotnet run

Write-Host ""
Write-Host "Done!" -ForegroundColor Green

