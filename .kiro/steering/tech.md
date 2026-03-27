# Tech Stack

## Language & Runtime
- C# / .NET 8

## Frameworks & Libraries
- **ASP.NET Core 8** — Web API framework
- **Entity Framework Core 8** (Npgsql) — ORM for PostgreSQL
- **MediatR 14** — CQRS mediator pattern
- **FluentValidation 12** — Request validation
- **ASP.NET Core Identity** — User management
- **StackExchange.Redis** — Redis client for session caching
- **Serilog** — Structured logging (file + console sinks)
- **Swashbuckle** — Swagger/OpenAPI documentation
- **AspNetCore.HealthChecks** — Health check endpoints for PostgreSQL and Redis

## Infrastructure
- **PostgreSQL 15** — Primary database (master + per-tenant)
- **Redis 7** — Session state cache
- **Docker / Docker Compose** — Local development environment

## Common Commands

### Run & Build
```bash
dotnet build
dotnet run --project src/SaaS.API
```

### Database Migrations
```bash
# Create migrations
dotnet ef migrations add InitialCatalog -c MasterDbContext -o Migrations -p src/SaaS.Infrastructure -s src/SaaS.API
dotnet ef migrations add InitialTenant -c TenantDbContext -o Persistence/Migrations/Tenant -p src/SaaS.Infrastructure -s src/SaaS.API

# Apply migrations
dotnet run --project src/SaaS.API -- --migrate-all       # both master + all tenants
dotnet run --project src/SaaS.API -- --migrate-master    # master DB only
dotnet run --project src/SaaS.API -- --migrate-tenants   # all tenant DBs only
```

### Docker
```bash
docker-compose up     # start PostgreSQL, Redis, and API
docker-compose down   # stop all services
```

### Tests
```bash
dotnet test
```
