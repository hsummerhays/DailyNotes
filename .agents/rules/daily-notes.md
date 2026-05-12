---
trigger: always_on
---

Use PowerShell instead of bash
Ask before committing or pushing to git

## Architecture

DailyNotes uses Clean Architecture with four projects and a strict dependency chain:

```
Core  ←  Infrastructure  ←  Application  ←  Api
```

- **DailyNotes.Core** — entities, interfaces, DTOs. Zero external dependencies.
- **DailyNotes.Infrastructure** — EF Core `DailyNotesDbContext`, `AuthService`, null provider stubs (`NullEmailProvider`, `NullFileStorageProvider`, `NullAiVisionProvider`, `NullSpeechProvider`).
- **DailyNotes.Application** — service interfaces + implementations. All services inherit `ApplicationServiceBase` which provides tenant-scoped query helpers and injects `DailyNotesDbContext` + `ITenantContext`. Also defines `ITenantContext`.
- **DailyNotes.Api** — thin controllers (one service per controller). `HttpTenantContext` implements `ITenantContext` by reading JWT claims via `IHttpContextAccessor`.

## Adding new features

1. Add entities/interfaces to `DailyNotes.Core`
2. Add a service interface `DailyNotes.Application/Services/I<Name>Service.cs`
3. Add the implementation `DailyNotes.Application/Services/<Name>Service.cs` (inherit `ApplicationServiceBase`)
4. Register `services.AddScoped<I<Name>Service, <Name>Service>()` in `DailyNotes.Api/Program.cs`
5. Inject the interface into the controller — **never inject `DailyNotesDbContext` directly into a controller**

## Tenant scoping

`ApplicationServiceBase` provides two helpers used in every service:

```csharp
TenantScoped<T>(query)      // WHERE TenantId = x AND UserId = y  (IHasTenantUser entities)
TenantOnlyScoped<T>(query)  // WHERE TenantId = x                 (IHasTenant entities: Tag, Quiz)
```

`ITenantContext` (scoped, resolved from `IHttpContextAccessor`) provides `UserId` and `TenantId` for the current request.

## Running tests

Tests use `CustomWebApplicationFactory` with an InMemory database and `TestAuthHandler` that accepts `X-User-Id` and `X-Tenant-Id` request headers as JWT claim substitutes.

```powershell
dotnet test
```
