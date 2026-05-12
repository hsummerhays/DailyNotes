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
    Write-Host ".NET Restore complete." -ForegroundColor Green
}

# Install Root dependencies (for concurrently, etc.)
Write-Host "Installing Root dependencies..."
npm install
if ($LASTEXITCODE -eq 0) {
    Write-Host "Root dependencies installed." -ForegroundColor Green
}

# Install Frontend dependencies
Write-Host "Installing Frontend dependencies..."
if (Test-Path "src/daily-notes-ui/package.json") {
    npm install -w src/daily-notes-ui
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Frontend dependencies installed." -ForegroundColor Green
    }
}

# Ensure .env exists
if (-not (Test-Path ".env")) {
    Write-Host "Creating default .env file..."
    @'
# Default environment variables for DailyNotes
JWT_KEY=REPLACED_JWT_KEY
POSTGRES_PASSWORD=password
POSTGRES_USER=postgres
POSTGRES_DB=DailyNotes
'@ | Out-File -FilePath ".env" -Encoding utf8
    Write-Host ".env file created." -ForegroundColor Green
}

# Run Database Migrations (requires Docker/Postgres to be up)
Write-Host "Would you like to start Postgres and run database migrations? (y/n)"
$response = Read-Host
if ($response -eq 'y') {
    Write-Host "Starting Postgres container..."
    docker-compose up -d postgres
    Write-Host "Waiting for Postgres to be ready..."
    Start-Sleep -Seconds 5
    Write-Host "Applying migrations..."
    dotnet ef database update --project src/DailyNotes.Infrastructure/DailyNotes.Infrastructure.csproj --startup-project src/DailyNotes.Api/DailyNotes.Api.csproj
}

Write-Host "Initialization complete." -ForegroundColor Green
