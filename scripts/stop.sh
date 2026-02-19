#!/bin/bash

echo "Stopping DailyNotes services..."

# Stop Docker containers
if command -v docker-compose &> /dev/null; then
    echo "Shutting down Docker containers..."
    docker-compose down
fi

# Stop local API and Frontend processes
echo "Cleaning up local processes..."

# Stop dotnet API processes
pkill -f "dotnet run --project src/DailyNotes.Api"

# Stop vite/node processes
pkill -f "vite"

echo "Done. Services have been stopped."
