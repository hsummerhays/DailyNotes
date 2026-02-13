# Check if Docker is installed
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Warning "Docker is not installed or not in your PATH. Some features may not work."
}
else {
    Write-Host "Docker is available."
}

# Restore .NET dependencies
Write-Host "Restoring .NET dependencies..."
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "Restore complete." -ForegroundColor Green
}
else {
    Write-Error "Restore failed."
}

# Install Frontend dependencies
Write-Host "Installing Frontend dependencies..."
if (Test-Path "src/daily-notes-ui/package.json") {
    Push-Location "src/daily-notes-ui"
    npm install
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Frontend dependencies installed." -ForegroundColor Green
    }
    else {
        Write-Error "Frontend install failed."
    }
    Pop-Location
}
else {
    Write-Warning "Frontend package.json not found."
}
