param (
    [switch]$Local
)

Write-Host "Ensuring a clean state..."
& "$PSScriptRoot\stop.ps1"

if ($Local) {
    Write-Host "Starting Postgres container..."
    docker-compose up -d postgres
    
    Write-Host "Running DailyNotes API locally (new window)..."
    # Start API in a new PowerShell window so it doesn't block
    Start-Process powershell -ArgumentList "-Command", "dotnet run --project src/DailyNotes.Api/DailyNotes.Api.csproj"
}
else {
    Write-Host "Starting DailyNotes with Docker Compose..."
    docker-compose up -d --build
}

# Start Frontend
Write-Host "Starting Frontend..."
if (Test-Path "src/daily-notes-ui") {
    Write-Host "Starting Frontend (new window)..."
    Start-Process powershell -WorkingDirectory "src/daily-notes-ui" -ArgumentList "-Command", "npm run dev"
}
else {
    Write-Warning "Frontend directory src/daily-notes-ui not found."
}
