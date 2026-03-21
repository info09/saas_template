# SaaS .NET 8 Clean Architecture Template

> A production-ready, highly scalable Multi-Tenant SaaS template built on .NET 8 with Clean Architecture, CQRS, and Entity Framework Core.

## Quick Start
1. Ensure you have **.NET 8 SDK** and **PostgreSQL** installed.
2. Clone this repository and open `SaaS.sln`.
3. Update `CatalogConnection` in `SaaS.API/appsettings.json` with your PostgreSQL credentials.
4. Open the terminal and apply EF Core migrations:
   ```bash
   dotnet ef database update -c CatalogDbContext -p SaaS.Infrastructure -s SaaS.API
   ```
5. Run the API:
   ```bash
   dotnet run --project SaaS.API
   ```
6. Visit `https://localhost:xxxx/swagger` to explore the API APIs.

## Features
- **Clean Architecture**: Domain, Application, Infrastructure, and API layers strictly separated.
- **True Multi-Tenancy**: Database-per-tenant architecture ensuring complete data isolation.
- **CQRS Pattern**: Handled seamlessly via `MediatR` reducing controller bloat.
- **Automated Provisioning**: Creating a Tenant automatically spawns a Database and Seeds default users dynamically.
- **Robust Validation**: Pre-configured `FluentValidation` pipelines inside Application layer.
- **Security First**: ASP.NET Core Identity combined with JWT Token Authentication.

## Configuration
Edit `SaaS.API/appsettings.json` to change connection strings or JWT configuration.

| Variable | Description | Default |
|----------|-------------|---------|
| `CatalogConnection` | Primary Connection String to the Master DB holding all Tenant configurations. | `Host=localhost;Database=SaaS_MasterDb;Username=postgres;Password=your_password` |
| `Jwt:Key` | The extremely long secret key used to sign JWT tokens. | (Secret Key String) |

## API Reference
Requests to tenant-specific logic should include the HTTP Header:
`X-Tenant-Id: <id_of_tenant>`

Key Modules:
- **Tenant API (`/api/tenant`)**: Create and dynamically spin up a Tenant SQL Schema, or get the current resolved tenant context.
- **Todo API (`/api/todoitems`)**: Demonstrates a complete CQRS CRUD workflow under strict Tenant Isolation and Pagination logic.
- **Auth API (`/api/auth`)**: Authentication placeholder for user login `POST /api/auth/login`.

## Architecture
See [HUONG_DAN_SU_DUNG.md](./HUONG_DAN_SU_DUNG.md) for detailed architecture components and development workflows.

## License
MIT
