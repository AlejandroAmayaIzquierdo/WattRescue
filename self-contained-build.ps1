# WattRescue Self-Contained Build Script
# This script builds the application and prepares the database

param(
    [string]$Runtime = "win-x64",
    [string]$OutputDir = "./publish",
    [string]$Configuration = "Release"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  WattRescue Self-Contained Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean previous publish
if (Test-Path $OutputDir) {
    Write-Host "[1/4] Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $OutputDir
}

# Restore dependencies
Write-Host "[2/4] Restoring dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to restore dependencies" -ForegroundColor Red
    exit 1
}

# Publish self-contained
Write-Host "[3/4] Publishing self-contained application ($Runtime)..." -ForegroundColor Yellow
dotnet publish -c $Configuration -r $Runtime --self-contained true -o $OutputDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to publish application" -ForegroundColor Red
    exit 1
}

# Apply migrations to create fresh database
Write-Host "[4/4] Creating database with migrations..." -ForegroundColor Yellow

# Create a temporary database in the publish folder
$originalDb = "./wattrescue.db"
$publishDb = "$OutputDir/wattrescue.db"

# Remove existing db if present
if (Test-Path $originalDb) {
    Remove-Item -Force $originalDb
}

# Apply migrations using EF Core tools
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Failed to apply migrations with EF tools. Trying alternative..." -ForegroundColor Yellow
    
    # Alternative: Run the app briefly to apply migrations on startup
    # This requires the app to apply migrations on startup
}

# Copy database to publish folder
if (Test-Path $originalDb) {
    Copy-Item $originalDb $publishDb -Force
    Write-Host "Database copied to publish folder" -ForegroundColor Green
} else {
    Write-Host "WARNING: Database not found. It will be created on first run." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Build completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output: $OutputDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the application:" -ForegroundColor White
Write-Host "  cd $OutputDir" -ForegroundColor Gray
Write-Host "  ./WattRescue.exe" -ForegroundColor Gray
Write-Host ""