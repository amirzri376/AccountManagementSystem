# Build Account Management System Angular project
Write-Host "Building Account Management System Angular project..." -ForegroundColor Green
ng build

# Move files from browser subfolder to wwwroot
Write-Host "Moving files to correct location..." -ForegroundColor Green
if (Test-Path "../wwwroot/browser") {
    Move-Item "../wwwroot/browser/*" "../wwwroot/" -Force
    Remove-Item "../wwwroot/browser" -Force
    Write-Host "Files moved successfully!" -ForegroundColor Green
} else {
    Write-Host "No browser folder found, files may already be in correct location." -ForegroundColor Yellow
}

Write-Host "Build complete!" -ForegroundColor Green 