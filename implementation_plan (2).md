# Cross-Platform Development Support Plan

This plan aims to make the "DailyNotes" project easy to build and run on Windows, Mac, and Linux by introducing Development Containers and standardizing build/run scripts.

## User Review Required

> [!NOTE]
> I will be adding a `.devcontainer` configuration which requires Docker and VS Code (or GitHub Codespaces). This is the most robust way to ensure a consistent environment across platforms.

> [!IMPORTANT]
> I will create a `scripts/` directory containing both Bash (`.sh`) and PowerShell (`.ps1`) scripts for common tasks (`init`, `build`, `run`). This ensures users on any OS can run the same commands.

## Proposed Changes

### Root Directory

#### [NEW] [.devcontainer/devcontainer.json](file:///c:/HughApps/DailyNotes/.devcontainer/devcontainer.json)
- Configures a Dev Container using the official .NET 8 image.
- Installs necessary VS Code extensions (C#, Docker).
- Sets up port forwarding and environment variables.

#### [NEW] [README.md](file:///c:/HughApps/DailyNotes/README.md)
- detailed "Getting Started" guide for Windows, Mac, and Linux.
- Instructions for using the new scripts and Dev Container.

### Scripts Directory (`scripts/`)

#### [NEW] [scripts/init.sh](file:///c:/HughApps/DailyNotes/scripts/init.sh) & [scripts/init.ps1](file:///c:/HughApps/DailyNotes/scripts/init.ps1)
- Restores .NET dependencies.
- Checks for Docker availability.

#### [NEW] [scripts/build.sh](file:///c:/HughApps/DailyNotes/scripts/build.sh) & [scripts/build.ps1](file:///c:/HughApps/DailyNotes/scripts/build.ps1)
- Builds the solution/project using `dotnet build`.

#### [NEW] [scripts/run.sh](file:///c:/HughApps/DailyNotes/scripts/run.sh) & [scripts/run.ps1](file:///c:/HughApps/DailyNotes/scripts/run.ps1)
- Wrapper around `docker-compose up` to start the database and API.
- Alternatively, provides a mode to run locally with `dotnet run` if preferred (will default to Docker for consistency).

## Verification Plan

### Automated Tests
- I will run the newly created scripts on the current environment (Windows) to verify the `.ps1` scripts work.
- I will verify the shell scripts syntax (though I cannot execute them natively on Windows without WSL, I will ensure they follow standard POSIX/Bash best practices).

### Manual Verification
- **Windows**: Run `.\scripts\build.ps1` and verify it builds.
- **Dev Container**: Open the project in a Dev Container (if the user is willing to test, or I can verifying the config file validity).
