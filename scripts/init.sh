#!/bin/bash
set -e

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "Warning: Docker is not installed or not in your PATH. Some features may not work."
else
    echo "Docker is available."
fi

# Restore .NET dependencies
echo "Restoring .NET dependencies..."
dotnet restore
echo "Restore complete."

# Install Frontend dependencies
echo "Installing Frontend dependencies..."
if [ -f "src/daily-notes-ui/package.json" ]; then
    cd src/daily-notes-ui
    npm install
    echo "Frontend dependencies installed."
    cd ../.. # Go back to root
else
    echo "Warning: Frontend package.json not found."
fi
