#!/bin/bash
set -e

# Build the main solution
echo "Building DailyNotes solution..."
dotnet build DailyNotes.slnx --configuration Release
echo "Build complete."
