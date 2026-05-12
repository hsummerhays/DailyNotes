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
Write-Host "Stopping node/vite processes..."
$nodeTasks = Get-CimInstance Win32_Process -Filter "Name = 'node.exe'" | Where-Object { $_.CommandLine -like "*vite*" -or $_.CommandLine -like "*daily-notes-ui*" }
if ($nodeTasks) {
    foreach ($task in $nodeTasks) {
        Stop-Process -Id $task.ProcessId -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Done. Services have been stopped."
