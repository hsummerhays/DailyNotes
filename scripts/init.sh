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
