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

if [ "$MODE" == "local" ]; then
    echo "Running DailyNotes API locally..."
    dotnet run --project src/DailyNotes.Api/DailyNotes.Api.csproj
else
    echo "Starting DailyNotes with Docker Compose..."
    docker-compose up --build
fi
