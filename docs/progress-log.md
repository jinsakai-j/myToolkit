# Progress Log

## 2026-06-28

### Sprint
Sprint 0

### Progress
- Setup .NET solution structure.
- Setup ASP.NET Core Web API project.
- Setup Core and Infrastructure projects.
- Setup xUnit test projects.
- Setup React + Vite + TypeScript frontend.
- Setup PostgreSQL Docker Compose service.
- Setup EF Core DbContext and initial entities.
- Added initial EF Core migration source.
- Added Health Endpoint.
- Added Swagger/OpenAPI and CORS foundation.
- Added README, environment example, and ethical guidelines.
- Verified frontend `npm run lint`.
- Verified frontend `npm run build`.
- Verified `docker compose config`.

### Files Added
- `.env.example`
- `.gitignore`
- `README.md`
- `docker-compose.yml`
- `backend/.config/dotnet-tools.json`
- `backend/OsintToolkit.sln`
- `backend/src/OsintToolkit.Api/OsintToolkit.Api.csproj`
- `backend/src/OsintToolkit.Api/Program.cs`
- `backend/src/OsintToolkit.Api/Controllers/HealthController.cs`
- `backend/src/OsintToolkit.Api/Contracts/Responses/HealthResponse.cs`
- `backend/src/OsintToolkit.Api/Middleware/ExceptionHandlingMiddleware.cs`
- `backend/src/OsintToolkit.Api/appsettings.json`
- `backend/src/OsintToolkit.Api/appsettings.Development.json`
- `backend/src/OsintToolkit.Api/Properties/launchSettings.json`
- `backend/src/OsintToolkit.Core/OsintToolkit.Core.csproj`
- `backend/src/OsintToolkit.Core/Entities/Scan.cs`
- `backend/src/OsintToolkit.Core/Entities/ScanResult.cs`
- `backend/src/OsintToolkit.Core/Entities/Report.cs`
- `backend/src/OsintToolkit.Core/Enums/TargetType.cs`
- `backend/src/OsintToolkit.Core/Enums/ScanStatus.cs`
- `backend/src/OsintToolkit.Core/Enums/ModuleStatus.cs`
- `backend/src/OsintToolkit.Core/Interfaces/IAppDbContext.cs`
- `backend/src/OsintToolkit.Core/Services/TargetClassifier.cs`
- `backend/src/OsintToolkit.Infrastructure/OsintToolkit.Infrastructure.csproj`
- `backend/src/OsintToolkit.Infrastructure/DependencyInjection.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/AppDbContext.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/Configurations/ScanConfiguration.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/Configurations/ScanResultConfiguration.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/Configurations/ReportConfiguration.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/Migrations/20260628000000_InitialCreate.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs`
- `backend/tests/OsintToolkit.Api.Tests/OsintToolkit.Api.Tests.csproj`
- `backend/tests/OsintToolkit.Api.Tests/HealthEndpointTests.cs`
- `backend/tests/OsintToolkit.Core.Tests/OsintToolkit.Core.Tests.csproj`
- `backend/tests/OsintToolkit.Core.Tests/TargetClassifierTests.cs`
- `backend/tests/OsintToolkit.Infrastructure.Tests/OsintToolkit.Infrastructure.Tests.csproj`
- `backend/tests/OsintToolkit.Infrastructure.Tests/AppDbContextTests.cs`
- `frontend/package.json`
- `frontend/package-lock.json`
- `frontend/index.html`
- `frontend/vite.config.ts`
- `frontend/tsconfig.json`
- `frontend/tsconfig.app.json`
- `frontend/tsconfig.node.json`
- `frontend/src/main.tsx`
- `frontend/src/App.tsx`
- `frontend/src/vite-env.d.ts`
- `frontend/src/api/client.ts`
- `frontend/src/api/health.ts`
- `frontend/src/components/StatusBadge.tsx`
- `frontend/src/pages/Dashboard.tsx`
- `frontend/src/styles/main.css`
- `frontend/src/types/health.ts`
- `docs/ethical-guidelines.md`
- `docs/architecture.md`
- `reports/generated/.gitkeep`

### Files Modified
- `docs/project-plan.md`
- `README.md`

### Commit
feat: scaffold sprint 0 project foundation

### Next Task
- Install .NET 9 SDK in the local environment if not already installed.
- Run `dotnet restore` and `dotnet test`.
- Apply EF Core migration to PostgreSQL.
- Run backend API and verify `GET /api/health`.
- Run frontend dev server and verify dashboard health status.
- Start Sprint 1 scan management after Sprint 0 runtime verification is complete.

## 2026-06-28

### Sprint
Sprint 0

### Progress
- Installed .NET 9 SDK locally under `.dotnet/` for verification because global `dotnet` was not available.
- Regenerated `backend/OsintToolkit.sln` with the .NET CLI.
- Normalized project references to use portable `/` paths.
- Switched `OsintToolkit.Api` from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk` with `Microsoft.AspNetCore.App` framework reference because the Web SDK path failed in this sandbox without useful diagnostics.
- Added explicit ASP.NET Core usings required after switching API project SDK.
- Added `Microsoft.EntityFrameworkCore.Design` to API startup project for `dotnet ef`.
- Regenerated initial EF Core migration from the actual model.
- Updated PostgreSQL host port from `5432` to `5433` because local PostgreSQL already occupied `5432`.
- Verified `dotnet restore`.
- Verified `dotnet build`.
- Verified `dotnet test` with `-m:1` and elevated execution because VSTest needs local socket communication.
- Started PostgreSQL with Docker Compose.
- Applied EF Core migration successfully.
- Verified database tables: `scans`, `scan_results`, `reports`, and `__EFMigrationsHistory`.
- Started backend API and verified `GET /api/health` returned HTTP 200.
- Verified Swagger JSON at `/swagger/v1/swagger.json`.
- Updated Swagger UI configuration, but still needs restart and final verification next time.

### Files Added
- `backend/src/OsintToolkit.Infrastructure/Data/Migrations/20260628090831_InitialCreate.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/Migrations/20260628090831_InitialCreate.Designer.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs`

### Files Modified
- `.gitignore`
- `.env.example`
- `README.md`
- `docker-compose.yml`
- `backend/OsintToolkit.sln`
- `backend/src/OsintToolkit.Api/OsintToolkit.Api.csproj`
- `backend/src/OsintToolkit.Api/Program.cs`
- `backend/src/OsintToolkit.Api/Controllers/HealthController.cs`
- `backend/src/OsintToolkit.Api/Middleware/ExceptionHandlingMiddleware.cs`
- `backend/src/OsintToolkit.Api/appsettings.json`
- `backend/src/OsintToolkit.Infrastructure/Data/Configurations/ScanConfiguration.cs`
- `backend/src/OsintToolkit.Infrastructure/Data/Configurations/ScanResultConfiguration.cs`
- `backend/tests/OsintToolkit.Api.Tests/HealthEndpointTests.cs`
- `backend/tests/OsintToolkit.Core.Tests/TargetClassifierTests.cs`
- `backend/tests/OsintToolkit.Infrastructure.Tests/AppDbContextTests.cs`
- `docs/project-plan.md`
- `docs/architecture.md`
- `docs/progress-log.md`

### Commit
feat: verify sprint 0 foundation

### Next Task
- Rebuild backend after the latest Swagger UI route change.
- Start backend API again.
- Verify `GET /api/health` still returns HTTP 200.
- Verify Swagger UI at `/swagger` or `/swagger/index.html`.
- Start frontend dev server with `npm run dev`.
- Open/verify dashboard can call `GET /api/health` and display backend status.
- Update final Sprint 0 documentation after frontend and Swagger UI are verified.
- Do not start Sprint 1 until the remaining verification is complete.
