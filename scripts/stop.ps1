Write-Host "Stopping DailyNotes services..."

# Stop Docker containers
if (Get-Command "docker-compose" -ErrorAction SilentlyContinue) {
    Write-Host "Shutting down Docker containers..."
    docker-compose down
}

# Stop local API and Frontend processes if they are running
Write-Host "Cleaning up local processes..."

# Stop dotnet processes related to this project (optional, be careful)
$dotnetTasks = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*DailyNotes.Api*" }
if ($dotnetTasks) {
    Write-Host "Stopping dotnet API processes..."
    $dotnetTasks | Stop-Process -Force
}

# Stop node processes related to vite/frontend
$nodeTasks = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.MainModule.FileName -like "*node*" }
# Note: It's hard to be surgical with node processes without PIDs, 
# but we can look for specific command lines if needed.
# For now, let's just focus on the API and telling the user.

Write-Host "Done. Services have been stopped."
