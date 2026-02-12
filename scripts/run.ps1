param (
    [switch]$Local
)

if ($Local) {
    Write-Host "Running DailyNotes API locally..."
    # Assuming start project is DailyNotes.Api
    dotnet run --project src/DailyNotes.Api/DailyNotes.Api.csproj
}
else {
    Write-Host "Starting DailyNotes with Docker Compose..."
    docker-compose up --build
}
