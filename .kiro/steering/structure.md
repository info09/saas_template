# Project Structure

## Solution Layout

```
SaaS.sln
├── SaaS.Domain/          # Core domain — entities, interfaces, domain exceptions
├── SaaS.Application/     # Business logic — CQRS features, DTOs, validators, behaviors
├── SaaS.Infrastructure/  # Data access — EF Core, Identity, repositories, services
└── SaaS.API/             # Presentation — controllers, middlewares, configuration
```

Dependencies flow inward: API → Application → Domain. Infrastructure implements Application interfaces.

## Layer Details

### SaaS.Domain
- `Entities/` — domain entities (e.g. `Tenant`)
- `Exceptions/` — domain-specific exception types

### SaaS.Application
- `Features/` — feature-based folders (e.g. `Auth/`, `Tenants/`)
  - Each feature contains its Command/Query, Handler, Validator, and DTOs co-located
- `Common/Behaviors/` — MediatR pipeline behaviors (e.g. `ValidationBehavior`)
- `Common/Models/` — shared response models (`Result<T>`)
- `Dtos/` — shared data transfer objects
- `Interfaces/` — application-level contracts (e.g. `ITenantService`, `ICurrentUserService`)
- `DependencyInjection.cs` — registers MediatR, validators, and application services

### SaaS.Infrastructure
- `Persistence/` — `MasterDbContext`, `TenantDbContext`, EF configurations, migrations
- `Identity/` — ASP.NET Core Identity setup and `AppUser` model
- `Repositories/` — repository implementations
- `Services/` — infrastructure service implementations (encryption, tenant, etc.)
- `DependencyInjection.cs` — registers EF contexts, repositories, and infrastructure services

### SaaS.API
- `Controllers/` — API controllers (one per feature area)
- `Middlewares/` — request pipeline middleware (exception handling, tenant resolution, session validation, tenant authorization)
- `Extensions/` — `IServiceCollection` and `WebApplication` extension methods for configuration
- `Options/` — strongly-typed configuration classes
- `Infrastructure/Swagger/` — Swagger customization
- `Program.cs` — app entry point; wires up services and middleware pipeline

## Naming Conventions

| Artifact | Pattern | Example |
|---|---|---|
| Command | `[Action]Command` | `LoginCommand` |
| Query | `Get[Entity]Query` | `GetSessionsQuery` |
| Handler | `[Command/Query]Handler` | `LoginCommandHandler` |
| Request DTO | `[Action]Request` | `LoginRequest` |
| Response DTO | `[Action]Response` | `LoginResponse` |
| Service interface | `I[Name]Service` | `ITenantService` |
| Service impl | `[Name]Service` | `TenantService` |

## Middleware Pipeline Order

1. `GlobalExceptionMiddleware`
2. `TenantResolutionMiddleware`
3. Authentication (JWT)
4. `SessionValidationMiddleware`
5. `TenantAuthorizationMiddleware`
6. Authorization
7. Controllers

## Multi-Tenancy

- Master DB (`MasterDbContext`) — tenant registry, shared data
- Tenant DB (`TenantDbContext`) — per-tenant isolated data, connection string resolved at runtime
- Tenant context comes from `X-Tenant-Id` request header
- `TenantDbContext` is registered as scoped and resolves its connection string via `ITenantService`
