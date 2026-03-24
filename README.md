# SaaS .NET 8 Clean Architecture Template

This is a ready-to-use template for building Software as a Service (SaaS) applications using .NET 8 Web API, adhering to Clean Architecture principles.

## Features
- **Clean Architecture**: Organized into Domain, Application, Infrastructure, and API layers.
- **Multi-tenancy**: Database-per-tenant architecture. 
- **CQRS**: Command Query Responsibility Segregation via `MediatR`.
- **Validation**: FluentValidation pipeline logic ready.
- **Authentication**: ASP.NET Core Identity with JWT Bearer Token integration.
- **Database**: PostgreSQL (via EF Core Npgsql).
- **Middlewares**: Custom Global Exception handling and Tenant Resolution (`X-Tenant-Id` header).

## Project Structure
- `SaaS.Domain`: Core entities (`Tenant`), interfaces, and domain exceptions.
- `SaaS.Application`: Business logic, CQRS behaviors, DTOs, and `ITenantService` interface.
- `SaaS.Infrastructure`: EF Core implementations (`CatalogDbContext` & `TenantDbContext`), `AppUser` Identity model, and dynamic Tenant resolution logic.
- `SaaS.API`: Controllers, JWT Authentication setup in `Program.cs`, and Middlewares.

## Getting Started
1. Open `SaaS.API/appsettings.json` and configure your `"CatalogConnection"` pointing to your PostgreSQL server.
2. **Apply EF Core Migrations**:
   Open a terminal in the solution folder and run:
   ```bash
   dotnet ef migrations add InitialCatalog -c CatalogDbContext -o Persistence/Migrations/Catalog -p SaaS.Infrastructure -s SaaS.API
   dotnet ef migrations add InitialTenant -c TenantDbContext -o Persistence/Migrations/Tenant -p SaaS.Infrastructure -s SaaS.API
   dotnet run --project SaaS.API -- --migrate-all
   ```
3. Run the API project (`dotnet run --project SaaS.API` or F5 in Visual Studio).
4. Use the Swagger UI to test endpoints. For tenant-specific requests, make sure to include the `X-Tenant-Id` Header.

Migration commands are now explicit and no longer run automatically during API startup:
- `dotnet run --project SaaS.API -- --migrate-master`
- `dotnet run --project SaaS.API -- --migrate-tenants`
- `dotnet run --project SaaS.API -- --migrate-all`
