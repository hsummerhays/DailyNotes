# Build the main solution
Write-Host "Building DailyNotes solution..."
dotnet build DailyNotes.slnx --configuration Release
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build complete." -ForegroundColor Green
}
else {
    Write-Error "Build failed."
}
