# DailyNotes Environment Setup Guide

This document outlines the local environment configuration, secrets management, docker setup, and the dependency structure for both the API and UI to help you get `DailyNotes` up and running.

---

## 1. Prerequisites
- **.NET 10 SDK**
- **Node.js** (v18+ recommended) & **npm**
- **Docker Desktop** (used for local PostgreSQL database and optionally running the entire stack)
- **VS Code** (Optional but recommended, especially if using Dev Containers)

---

## 2. API Configuration (`appsettings.json`)
The DailyNotes core backend uses `appsettings.json` and `appsettings.Development.json` for managing its runtime configuration.

### Database Connection
By default, the application is configured to connect to a local PostgreSQL instance:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=DailyNotes;Username=postgres;Password="
}
```
*Note: If running as a Docker Compose service, the host is overridden via environment variables to `Host=postgres`.*

### Authentication (JWT)
The application currently uses ASP.NET Core Identity with JWT bearer tokens. 
```json
"Jwt": {
  "Key": "[REDACTED_DEV_KEY]",
  "Issuer": "DailyNotesApi",
  "Audience": "DailyNotesClient"
}
```
**Important:** Do not use this development key in production. It should be replaced with a secure secret (e.g., Azure Key Vault) when deployed.

---

## 3. Docker & Services Setup
The project comes with a `docker-compose.yml` file to quickly stand up the required infrastructure.

### Running PostgreSQL locally
If you wish to run the .NET API directly on your host machine while containerizing the database, you can start just the database:
```powershell
docker-compose up -d postgres
```
This spins up a PG16 instance mapped to your local `5432` port with the username `postgres` and password `password`. Data is persisted to a Docker volume (`postgres_data`).

### Running the Full Stack
If you prefer running the API within Docker alongside the database:
```powershell
docker-compose up -d
```
The API container will be built and exposed on port `5010`.

---

## 4. Frontend Workspace & Dependencies (NPM Packages)

The frontend application is built as an npm workspace located in the `src/daily-notes-ui` directory. The root directory manages the workspace and scripts.

To install all of the required packages for the workspace, you simply need to run the following command from the root project directory:
```powershell
npm install
# OR
npm run install:all
```

Below is the documented list of npm packages that are installed for the `daily-notes-ui` project.

### Core Dependencies
These packages are required for the application to run successfully in production.

#### React & Routing
- **`react` / `react-dom`** (`^19.2.0`): The core library for building user interfaces.
- **`react-router-dom`** (`^7.13.0`): Declarative routing for React applications.

#### State Management & Data Fetching
- **`zustand`** (`^5.0.11`): A small, fast, and scalable bearbones state-management solution.
- **`@tanstack/react-query`** (`^5.90.21`): Powerful asynchronous state management, server-state utilities, and data fetching for React.

#### Editor (Lexical)
Lexical is an extensible text editor framework used to provide rich text editing capabilities.
- **`lexical`** (`^0.41.0`): The core editor framework.
- Plugins: `@lexical/react`, `@lexical/rich-text`, `@lexical/plain-text`, `@lexical/history`, `@lexical/link`, `@lexical/list`, `@lexical/code`, `@lexical/clipboard`, `@lexical/selection`, `@lexical/utils`

#### Utilities & UI Components
- **`axios`** (`^1.13.5`): Promise-based HTTP client for API requests.
- **`date-fns`** (`^4.1.0`): Modern JavaScript date utility library.
- **`lucide-react`** (`^0.577.0`): A beautiful and consistent icon toolkit for React.
- **`prismjs`** (`^1.30.0`): Lightweight syntax highlighter (used for lexical code blocks).

### Development Dependencies (devDependencies)
These packages are only needed for local development, building, and testing.

#### Building & Tooling
- **`vite`** (`^7.3.1`): Next-generation frontend tooling. It's fast and provides a great developer experience.
- **`@vitejs/plugin-react`** (`^5.1.1`): Vite plugin for React projects.
- **`typescript`** (`~5.9.3`): TypeScript programming language.

#### Styling (Tailwind CSS)
- **`tailwindcss`** (`^4.2.1`): A utility-first CSS framework for rapid UI development.
- **`@tailwindcss/vite`** (`^4.2.1`): Vite integration plugin for Tailwind CSS.

#### Linting (ESLint)
- **`eslint`** (`^9.39.1`) & **`@eslint/js`**: Pluggable JavaScript linter.
- **`typescript-eslint`** (`^8.48.0`): Tooling which enables ESLint to support TypeScript.
- **`eslint-plugin-react-hooks`** / **`eslint-plugin-react-refresh`**: ESLint rules for React.

#### TypeScript Definitions
- **`@types/node`** (`^24.10.1`), **`@types/react`** (`^19.2.7`), **`@types/react-dom`** (`^19.2.3`), **`@types/prismjs`**
