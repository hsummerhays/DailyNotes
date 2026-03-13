# DailyNotes

DailyNotes is a cloud-native note-taking application designed for flexibility and ease of use.

## Architecture Principles

DailyNotes is designed using a few core architectural principles:

**Clean Separation of Concerns**  
Domain entities and business rules live in `DailyNotes.Core` and have no infrastructure dependencies.

**Cloud-Native by Default**  
The application is containerized and designed to run locally via Docker or in cloud environments with minimal changes.

**Database-First Migration Strategy**  
The system preserves the structure and relationships of the original FileMaker data while modernizing the application architecture.

**Multi-Tenant Ready**  
All domain entities include tenant boundaries, enabling SaaS deployment without redesign.

**Provider Abstraction**  
External services such as file storage, email, speech, and AI are accessed through interfaces so implementations can be swapped without affecting the domain layer.

## Tech Stack

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Used for local PostgreSQL and optional containerized stack)
- [React 19](https://react.dev/) / [Vite](https://vitejs.dev/) / [TypeScript](https://www.typescriptlang.org/)
- [PostgreSQL](https://www.postgresql.org/) / [Entity Framework Core 10](https://learn.microsoft.com/en-us/ef/core/)
- [VS Code](https://code.visualstudio.com/) (Recommended IDE)

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

### Development with Dev Containers

This project is configured for **Remote - Containers**. This allows you to develop inside a Docker container with all dependencies pre-installed.

1.  Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) in VS Code.
2.  Open the project folder in VS Code.
3.  Click "Reopen in Container" when prompted, or run the command `Dev Containers: Reopen in Container` from the Command Palette (`Ctrl+Shift+P`).

### Project Structure

- `src/`: Source code for the application.
- `docs/`: Project documentation.
- `scripts/`: Helper scripts for build and run tasks.
- `.devcontainer/`: Configuration for VS Code Dev Containers.
- `docker-compose.yml`: Definition for multi-container Docker application.

> [!WARNING]
> The default `docker-compose.yml` uses `password` as the PostgreSQL password. This is fine for local development, but **must be changed** before any real deployment. See the [Environment Setup Guide](docs/Environment_Setup_Guide.md) for configuration details.

## Docs

For a deeper dive into the architecture, future plans, and setting up the local environment, see the following documentation:
- [Environment Setup Guide](docs/Environment_Setup_Guide.md)
- [Architecture & Implementation Plan](docs/architecture.md)
- [Future Roadmap](docs/roadmap.md)
