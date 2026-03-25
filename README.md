# SaaS .NET 8 Clean Architecture Template

This is a ready-to-use template for building Software as a Service (SaaS) applications using .NET 8 Web API, adhering to Clean Architecture principles.

## Features
- **Clean Architecture**: Organized into Domain, Application, Infrastructure, and API layers.
- **Multi-tenancy**: Database-per-tenant architecture.
- **CQRS**: Command Query Responsibility Segregation via `MediatR`.
- **Validation**: FluentValidation validators and MediatR pipeline behaviors.
- **Authentication**: ASP.NET Core Identity with JWT Bearer Token integration.
- **Database**: PostgreSQL (via EF Core Npgsql).
- **Error Contract**: `ProblemDetails` and `ValidationProblemDetails` for API failures.
- **Middlewares**: Global exception handling, tenant resolution, and cross-tenant authorization checks.

## Project Structure
- `SaaS.Domain`: Core entities (`Tenant`), interfaces, and domain exceptions.
- `SaaS.Application`: Business logic, CQRS behaviors, DTOs, validators, and onboarding workflow services.
- `SaaS.Infrastructure`: EF Core implementations (`MasterDbContext` & `TenantDbContext`), `AppUser` Identity model, tenant provisioning, and repositories.
- `SaaS.API`: Controllers, JWT authentication setup in `Program.cs`, Swagger, and middlewares.

## Getting Started
1. Open `SaaS.API/appsettings.json` and configure your `"CatalogConnection"` pointing to your PostgreSQL server.
2. **Create or update EF Core migrations**:
   Open a terminal in the solution folder and run:
   ```bash
   dotnet ef migrations add InitialCatalog -c MasterDbContext -o Migrations -p SaaS.Infrastructure -s SaaS.API
   dotnet ef migrations add InitialTenant -c TenantDbContext -o Persistence/Migrations/Tenant -p SaaS.Infrastructure -s SaaS.API
   ```
3. **Apply database schema explicitly**:
   ```bash
   dotnet run --project SaaS.API -- --migrate-all
   ```
4. Run the API project (`dotnet run --project SaaS.API` or F5 in Visual Studio).
5. Use the Swagger UI to test endpoints. For tenant-specific requests, include the `X-Tenant-Id` header.

Migration commands are now explicit and no longer run automatically during API startup:
- `dotnet run --project SaaS.API -- --migrate-master`
- `dotnet run --project SaaS.API -- --migrate-tenants`
- `dotnet run --project SaaS.API -- --migrate-all`

## API Contract Notes
- `POST /api/auth/login` expects only `email` and `password` in the request body.
- `POST /api/auth/refresh` expects `refreshToken` in the request body.
- Tenant context for login and tenant-scoped APIs comes from the `X-Tenant-Id` header.
- Auth success responses now return `accessToken`, `refreshToken`, `accessTokenExpiresAtUtc`, and `refreshTokenExpiresAtUtc`.
- Success responses return `Result<T>`.
- Failure responses return `ProblemDetails` or `ValidationProblemDetails`.
