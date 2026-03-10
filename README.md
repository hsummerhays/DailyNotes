# DailyNotes

DailyNotes is a cloud-native note-taking application designed for flexibility and ease of use.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Optional, but recommended for consistent environment)
- [VS Code](https://code.visualstudio.com/) (Recommended)

## Documentation

For a deeper dive into the architecture, future plans, and setting up the local environment, see the following documentation:
- [Environment Setup Guide](docs/Environment_Setup_Guide.md)
- [Architecture & Implementation Plan](docs/architecture.md)
- [Future Roadmap](docs/roadmap.md)

## Getting Started

We provide helper scripts for Windows (PowerShell) and Mac/Linux (Bash) to make development easy.

### Windows

1.  **Initialize**: Restore dependencies.
    ```powershell
    .\scripts\init.ps1
    ```
2.  **Build**: compile the project.
    ```powershell
    .\scripts\build.ps1
    ```
3.  **Run**: Start the application.
    -   **With Docker** (Recommended):
        ```powershell
        .\scripts\run.ps1
        ```
    -   **Locally** (Without Docker):
        ```powershell
        .\scripts\run.ps1 -Local
        ```

### Mac / Linux

1.  **Initialize**: Restore dependencies.
    ```bash
    ./scripts/init.sh
    ```
2.  **Build**: compile the project.
    ```bash
    ./scripts/build.sh
    ```
3.  **Run**: Start the application.
    -   **With Docker** (Recommended):
        ```bash
        ./scripts/run.sh
        ```
    -   **Locally** (Without Docker):
        ```bash
        ./scripts/run.sh --local
        ```

## Development with Dev Containers

This project is configured for **Remote - Containers**. This allows you to develop inside a Docker container with all dependencies pre-installed.

1.  Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) in VS Code.
2.  Open the project folder in VS Code.
3.  Click "Reopen in Container" when prompted, or run the command `Dev Containers: Reopen in Container` from the Command Palette (`Ctrl+Shift+P`).

## Project Structure

- `src/`: Source code for the application.
- `docs/`: Project documentation.
- `scripts/`: Helper scripts for build and run tasks.
- `.devcontainer/`: Configuration for VS Code Dev Containers.
- `docker-compose.yml`: Definition for multi-container Docker application.

> [!WARNING]
> The default `docker-compose.yml` uses `password` as the PostgreSQL password. This is fine for local development, but **must be changed** before any real deployment. See the [Environment Setup Guide](docs/Environment_Setup_Guide.md) for configuration details.

