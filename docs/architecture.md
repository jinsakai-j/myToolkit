# Architecture

OSINT Toolkit Local uses a local-first architecture with a React frontend, ASP.NET Core Web API backend, PostgreSQL database, and EF Core persistence.

## System Overview

```text
React + Vite + TypeScript
  -> ASP.NET Core Web API
      -> Core
      -> Infrastructure
          -> PostgreSQL
```

## Backend Layers

### API

Project:

```text
backend/src/OsintToolkit.Api
```

Responsibilities:

- HTTP endpoints
- Swagger/OpenAPI
- CORS
- Request/response contracts
- Error handling middleware
- Dependency injection composition

### Core

Project:

```text
backend/src/OsintToolkit.Core
```

Responsibilities:

- Domain entities
- Enums
- Domain services
- Interfaces
- Business rules that do not depend on framework or database details

### Infrastructure

Project:

```text
backend/src/OsintToolkit.Infrastructure
```

Responsibilities:

- EF Core DbContext
- PostgreSQL configuration
- Entity configurations
- Migrations
- Future provider integrations

## Architecture Decision Record (ADR)

### ADR-001: Use ASP.NET Core Web API, React TypeScript, PostgreSQL, and EF Core

Date: 2026-06-28

Status: Accepted

Decision:

- Frontend uses `React + Vite + TypeScript`.
- Backend uses `ASP.NET Core Web API (.NET 9)`.
- Database uses `PostgreSQL`.
- ORM uses `Entity Framework Core`.
- Tests use `xUnit`.

Reason:

- The project is intended as a cybersecurity and software engineering portfolio, so a typed full-stack architecture is preferred.
- ASP.NET Core and EF Core provide strong structure for API, persistence, migrations, and testability.
- PostgreSQL is closer to production-grade relational database usage than SQLite while still easy to run locally via Docker Compose.
- TypeScript improves frontend maintainability and makes API contracts easier to reason about.

Consequences:

- Local development requires .NET 9 SDK, Node.js, npm, and Docker.
- EF Core migrations become the source of truth for database schema.
- Python remains optional and should only be introduced later if a specific OSINT library strongly justifies it.

### ADR-002: Keep Sprint 0 Free of Real OSINT Modules

Date: 2026-06-28

Status: Accepted

Decision:

- Sprint 0 only contains project foundation.
- DNS lookup, WHOIS lookup, username checker, IP reputation, report PDF generation, and Python worker are not implemented in Sprint 0.

Reason:

- Foundation work should establish maintainable architecture before feature work starts.
- Legal and ethical guardrails are easier to enforce when OSINT modules are added deliberately, one by one.

Consequences:

- The current frontend shows backend health and project status only.
- Scan management begins in Sprint 1.
- OSINT modules begin after the scan lifecycle exists.

### ADR-003: Use Scan-Centric API Design for MVP

Date: 2026-06-28

Status: Accepted

Decision:

- MVP endpoints should be centered around scans instead of separate OSINT module endpoints.
- Current Sprint 0 exposes only `GET /api/health`.

Reason:

- A scan-centric model keeps the API small while the product shape is still evolving.
- It avoids premature endpoint sprawl such as `/api/domain/dns`, `/api/domain/whois`, and separate module-specific controllers before scan orchestration exists.

Consequences:

- Sprint 1 should add scan CRUD endpoints.
- Module-specific endpoints should only be introduced later if scan-centric endpoints become insufficient.

### ADR-004: Use Standard .NET SDK for API Project in This Repository

Date: 2026-06-28

Status: Accepted

Decision:

- `OsintToolkit.Api` uses `Microsoft.NET.Sdk` with `OutputType` set to `Exe`.
- ASP.NET Core is referenced through `FrameworkReference Include="Microsoft.AspNetCore.App"`.
- `appsettings.json` and `appsettings.Development.json` are explicitly copied to output.
- `Program.cs` sets `ContentRootPath` to `AppContext.BaseDirectory`.

Reason:

- The original `Microsoft.NET.Sdk.Web` API project failed during restore/build in the current Linux sandbox without emitting actionable errors.
- The API does not currently need Razor, static web assets, or other Web SDK-specific build features.
- Standard SDK plus `Microsoft.AspNetCore.App` keeps the application as an ASP.NET Core Web API while making build behavior explicit and portable in this environment.

Consequences:

- ASP.NET Core namespaces that were previously implicit must be imported explicitly.
- Configuration files must be explicitly copied to the output directory.
- If future features need Web SDK-specific behavior, this decision can be revisited.

### ADR-005: Use Host Port 5433 for Docker PostgreSQL

Date: 2026-06-28

Status: Accepted

Decision:

- Docker Compose maps PostgreSQL container port `5432` to host port `5433`.
- Backend development connection strings use `localhost:5433`.

Reason:

- Host port `5432` was already occupied by a local PostgreSQL service.
- Keeping the existing local service untouched is safer than stopping user-owned database processes.

Consequences:

- Local PostgreSQL for this project is reached at `localhost:5433`.
- README and `.env.example` must continue to document port `5433`.
