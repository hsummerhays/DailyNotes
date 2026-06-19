# DailyNotes

DailyNotes is a multi-tenant personal productivity and knowledge management platform. It replaces a legacy FileMaker Pro work-tracking system with a modern cloud-native API and React SPA.

**Core capabilities:** work day and time tracking, task/project management, rich-text note-taking, knowledge base with hierarchical topics and quizzes, course/assignment tracking, and cross-entity tagging and search.

## Architecture

DailyNotes is a 4-layer Clean Architecture .NET 10 API backed by PostgreSQL, with a React 19 frontend served separately.

```
DailyNotes.Core          # Domain entities, interfaces, exceptions — no dependencies
DailyNotes.Infrastructure# EF Core, ASP.NET Identity, provider implementations
DailyNotes.Application   # Use-case services, ITenantContext, request DTOs
DailyNotes.Api           # Thin controllers, JWT auth, middleware
daily-notes-ui           # React 19 + Vite + TypeScript SPA
```

**Dependency direction:**

```
Core  ←  Infrastructure
Core  ←  Application  ←  Api
           ↑
     Infrastructure (IDailyNotesDataContext)
```

Application services never reference Infrastructure directly. They depend on `IDailyNotesDataContext` (defined in Application), which `DailyNotesDbContext` implements in Infrastructure.

See [docs/architecture.md](docs/architecture.md) for the full layer breakdown, schema, and endpoint reference.

## Architecture Principles

**Multi-Tenant by Default**
Every entity carries `TenantId`. Most also carry `UserId`. `ApplicationServiceBase` provides `TenantScoped<T>` and `TenantOnlyScoped<T>` helpers that all services use — cross-tenant data access is structurally prevented.

**Clean Separation of Concerns**
Domain entities and business rules live in `DailyNotes.Core` with no external dependencies. Controllers are 1–3 lines each. Services contain all use-case logic. Infrastructure is swappable.

**Provider Abstraction**
File storage, email, AI vision, and speech are accessed through interfaces (`IFileStorageProvider`, `IEmailProvider`, etc.) with no-op null implementations. Swap in real providers by registering them in `AddInfrastructureServices` without touching any service code.

**Cloud-Native by Default**
Containerized and runnable locally via Docker or against a standalone Postgres instance.

## Tech Stack

| Layer | Technology |
|---|---|
| API | .NET 10, ASP.NET Core, EF Core 10 |
| Database | PostgreSQL (JSONB for rich content fields) |
| Auth | ASP.NET Core Identity + JWT Bearer + refresh token rotation |
| Frontend | React 19, Vite 7, TypeScript, Tailwind CSS 4, Lexical |
| State | TanStack Query 5, Zustand 5 |
| Tests | xUnit, WebApplicationFactory, InMemory DB |

## Getting Started

Helper scripts are provided for Windows (PowerShell) and Mac/Linux (Bash).

### Windows

```powershell
# Restore dependencies
.\scripts\init.ps1

# Build
.\scripts\build.ps1

# Run (Docker recommended — starts API + Postgres)
.\scripts\run.ps1

# Run locally without Docker
.\scripts\run.ps1 -Local
```

### Mac / Linux

```bash
./scripts/init.sh
./scripts/build.sh
./scripts/run.sh          # Docker
./scripts/run.sh --local  # Without Docker
```

### Dev Container

The project includes a `.devcontainer/` configuration for VS Code. Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers), open the folder, and select **Reopen in Container**.

### Project Layout

```
src/                    # All source code
  DailyNotes.Api/       # Web API
  DailyNotes.Application/
  DailyNotes.Core/
  DailyNotes.Infrastructure/
  DailyNotes.Api.Tests/
  daily-notes-ui/       # React SPA
docs/                   # Architecture, setup, roadmap
scripts/                # Build and run helpers
docker-compose.yml      # Local dev stack
```

> [!WARNING]
> The default `docker-compose.yml` uses `password` as the PostgreSQL password. Change it before any real deployment.

## Future Improvements

### Near-term (production-readiness)

| Area | What |
|---|---|
| **Pagination envelope** | Return `{ items, total, page, pageSize }` instead of raw arrays so clients can render pagination controls |
| **Health checks** | `GET /health` endpoint with DB connectivity check via `AddHealthChecks().AddDbContext<>()` |
| **Observability** | Structured logging (Serilog) + OpenTelemetry traces and metrics; export to Seq or Azure Monitor |
| **Role enforcement** | `role` claim is in the JWT but never checked — add `[Authorize(Roles = "owner")]` guards on admin/destructive endpoints |
| **Refresh token expiry** | Add `ExpiresAt` to token store; reject and clean up stale refresh tokens |
| **Background jobs** | Webhook delivery and email digests need an out-of-request processor (Hangfire or Quartz.NET) |
| **Unit tests** | `IDailyNotesDataContext` is an interface — add unit tests with a mock context alongside the existing integration tests |
| **Full-text search** | Upgrade `SearchService` from `EF.Functions.Like` to `tsvector` GIN index queries (schema is already prepared) |
| **Optimistic concurrency** | Add `RowVersion` to frequently-edited entities and return `ETag` headers |
| **FluentValidation** | Replace DataAnnotations on request DTOs with `AbstractValidator<T>` classes for richer error responses |

See the full [roadmap](docs/roadmap.md) for the complete backlog including third-party integrations, AI/MCP features, communication bots, mobile, and more.

## Documentation

- [Architecture & Implementation Plan](docs/architecture.md)
- [Future Roadmap](docs/roadmap.md)
