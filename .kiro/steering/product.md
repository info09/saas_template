# Product

This is a **SaaS .NET 8 Clean Architecture Template** — a ready-to-use foundation for building multi-tenant Software as a Service applications.

## What it does

- Provides a multi-tenant backend API where each tenant gets an isolated PostgreSQL database
- Handles authentication (JWT + refresh tokens), session management (Redis-backed), and tenant-scoped authorization
- Exposes a RESTful API with Swagger documentation and health check endpoints

## Key capabilities

- Database-per-tenant isolation with a shared master database for tenant registry
- JWT authentication with session tracking and Redis-backed validation
- Tenant context resolved from `X-Tenant-Id` request header
- Explicit database migration commands (no auto-migration on startup)
- Containerized local dev environment via Docker Compose

## API contract

- Tenant context: `X-Tenant-Id` header required for all tenant-scoped requests
- Success responses: `Result<T>` wrapper
- Error responses: `ProblemDetails` or `ValidationProblemDetails` (RFC 7807)
- Auth tokens: `accessToken`, `refreshToken`, `accessTokenExpiresAtUtc`, `refreshTokenExpiresAtUtc`
- Health endpoints: `GET /health`, `GET /health/ready`
