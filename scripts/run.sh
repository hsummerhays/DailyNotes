#!/bin/bash
set -e

usage() {
    echo "Usage: $0 [--local]"
    exit 1
}

MODE="docker"

while [[ "$#" -gt 0 ]]; do
    case $1 in
        --local) MODE="local" ;;
        *) usage ;;
    esac
    shift
done

# Ensure a clean state
echo "Ensuring a clean state..."
"$(dirname "$0")/stop.sh"

if [ "$MODE" == "local" ]; then
    echo "Starting Postgres container..."
    docker-compose up -d postgres

    echo "Running DailyNotes API locally (background)..."
    dotnet run --project src/DailyNotes.Api/DailyNotes.Api.csproj &
    API_PID=$!
    
    # Trap Ctrl+C to kill the API process when frontend stops
    trap "kill $API_PID" EXIT
else
    echo "Starting DailyNotes with Docker Compose..."
    docker-compose up -d --build
fi

# Start Frontend
echo "Starting Frontend..."
if [ -d "src/daily-notes-ui" ]; then
    cd src/daily-notes-ui
    npm run dev &
    FRONT_PID=$!
    echo "Frontend started (PID: $FRONT_PID). Use 'kill $FRONT_PID' to stop."
    
    if [ "$MODE" == "local" ]; then
         # Wait for both processes on Ctrl+C
         wait $FRONT_PID $API_PID
    else
         # In Docker mode, only wait for frontend
         wait $FRONT_PID
    fi
else
    echo "Warning: Frontend directory src/daily-notes-ui not found."
fi
