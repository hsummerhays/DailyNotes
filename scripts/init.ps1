# Check if Docker is installed
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Warning "Docker is not installed or not in your PATH. Some features may not work."
} else {
    Write-Host "Docker is available."
}

# Restore .NET dependencies
Write-Host "Restoring .NET dependencies..."
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "Restore complete." -ForegroundColor Green
} else {
    Write-Error "Restore failed."
}
