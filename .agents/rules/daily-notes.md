---
trigger: always_on
---

Use PowerShell instead of bash
Ask before committing or pushing to git

## Architecture

DailyNotes uses Clean Architecture with four projects and a strict dependency chain:

```
Core  ←  Infrastructure
Core  ←  Application  ←  Api
           ↑
     Infrastructure (via IDailyNotesDataContext)
```

- **DailyNotes.Core** — entities, interfaces, DTOs. Zero external dependencies.
- **DailyNotes.Infrastructure** — EF Core `DailyNotesDbContext`, `AuthService`, null provider stubs (`NullEmailProvider`, `NullFileStorageProvider`, `NullAiVisionProvider`, `NullSpeechProvider`).
- **DailyNotes.Application** — service interfaces + implementations. All services inherit `ApplicationServiceBase` which provides tenant-scoped query helpers and injects `DailyNotesDbContext` + `ITenantContext`. Also defines `ITenantContext`.
- **DailyNotes.Api** — thin controllers (one service per controller). `HttpTenantContext` implements `ITenantContext` by reading JWT claims via `IHttpContextAccessor`.

## Adding new features

1. Add entities/interfaces to `DailyNotes.Core`
2. Add a service interface `DailyNotes.Application/Services/I<Name>Service.cs`
3. Add the implementation `DailyNotes.Application/Services/<Name>Service.cs` (inherit `ApplicationServiceBase`)
4. Register `services.AddScoped<I<Name>Service, <Name>Service>()` in `DailyNotes.Api/Extensions/ServiceCollectionExtensions.cs` → `AddApplicationServices()`
5. Inject the interface into the controller — **never inject `DailyNotesDbContext` directly into a controller**

## Tenant scoping

`ApplicationServiceBase` provides two helpers used in every service:

```csharp
TenantScoped<T>(query)      // WHERE TenantId = x AND UserId = y  (IHasTenantUser entities)
TenantOnlyScoped<T>(query)  // WHERE TenantId = x                 (IHasTenant entities: Tag, Quiz)
```

`ITenantContext` (scoped, resolved from `IHttpContextAccessor`) provides `UserId` and `TenantId` for the current request.

As a second-layer safety net, `DailyNotesDbContext` applies EF Core global query filters (`HasQueryFilter`) to all 13 multi-tenant entities. A missing `TenantScoped` call will not leak cross-tenant data. The filter is bypassed automatically when no HTTP context is present (migrations, background jobs).

## CancellationToken

All service interface methods accept `CancellationToken ct = default` as the last parameter. Thread `ct` through to every EF Core async call (`ToListAsync(ct)`, `FirstOrDefaultAsync(pred, ct)`, `SaveChangesAsync(ct)`, etc.). Controllers forward `HttpContext.RequestAborted`.

## Running tests

Tests use `CustomWebApplicationFactory` with an InMemory database and `TestAuthHandler` that accepts `X-User-Id` and `X-Tenant-Id` request headers as JWT claim substitutes.

```powershell
dotnet test
```
