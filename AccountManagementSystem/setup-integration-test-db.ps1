# Setup Integration Test Database
Write-Host "Setting up Integration Test Database..." -ForegroundColor Green

# Set environment to Test
$env:ASPNETCORE_ENVIRONMENT = "Test"

# Navigate to the AccountManagementSystem directory
Set-Location $PSScriptRoot

# Create the database and apply migrations
Write-Host "Creating database and applying migrations..." -ForegroundColor Yellow
dotnet ef database update --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Integration test database setup completed successfully!" -ForegroundColor Green
    Write-Host "Database: LoginSystemDB_IntegrationTests" -ForegroundColor Cyan
    Write-Host "Server: (localdb)\mssqllocaldb" -ForegroundColor Cyan
} else {
    Write-Host "❌ Database setup failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nYou can now run integration tests with:" -ForegroundColor Yellow
Write-Host "dotnet test IntegrationTests/IntegrationTests.csproj --verbosity normal" -ForegroundColor White
