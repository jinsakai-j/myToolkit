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

## 2026-06-28

### Sprint
Sprint 0

### Progress
- Audited repository hygiene before continuing Sprint 0.
- Confirmed tracked files do not include `.dotnet/`, `node_modules/`, `dist/`, `bin/`, `obj/`, coverage output, or generated report PDFs.
- Confirmed ignored local generated folders exist in the workspace: `.dotnet/`, backend `bin/obj`, `frontend/node_modules/`, and `frontend/dist/`.
- Checked for obvious secrets and found only placeholder local PostgreSQL credentials in `.env.example`, `docker-compose.yml`, and development appsettings.
- Expanded `.gitignore` for logs, local appsettings, Vite cache, broader env files, symbols, and generated PDF reports.
- Updated README with repository hygiene notes and clearer PostgreSQL port/test guidance.

### Files Added
- None

### Files Modified
- `.gitignore`
- `README.md`
- `docs/progress-log.md`

### Commit
chore: tidy repository hygiene documentation

### Next Task
- Verify the local environment and runtime of Sprint 0 foundation (database, backend, frontend, Swagger UI, and CORS).
- Do not start Sprint 1 until Sprint 0 runtime verification is complete.

## 2026-07-08

### Sprint
Sprint 0

### Progress
- Created root `.env` config file from `.env.example`.
- Solved container daemon connection by unsetting `DOCKER_HOST` environment variable to connect directly to the active system Docker service.
- Started PostgreSQL service container using Docker Compose mapped to host port 5433.
- Executed `dotnet ef database update` to successfully apply initial EF Core migrations.
- Ran all backend tests across Api, Core, and Infrastructure projects (all passed).
- Launched backend API and verified `GET /api/health` returns status `Healthy`.
- Verified Swagger UI HTML page loads correctly at `http://localhost:5080/swagger/index.html`.
- Launched frontend Vite development server at `http://localhost:5173/`.
- Verified frontend-to-backend CORS connection by sending requests with the local Origin header.
- Updated TypeScript configuration files (`tsconfig.app.json` and `tsconfig.node.json`) to use the modern `"bundler"` module resolution strategy to eliminate `node10` deprecation warnings.
- Fully completed Sprint 0 verification.

### Files Added
- `.env`

### Files Modified
- `docs/progress-log.md`
- `frontend/tsconfig.app.json`
- `frontend/tsconfig.node.json`

### Commit
chore: verify sprint 0 runtime and update typescript configuration

### Next Task
- Start Sprint 1 - Scan Management.
- Implement DNS lookup, WHOIS lookup, Username checker, and IP reputation modules.

## 2026-07-15

### Sprint
Sprint 1

### Progress
- Implemented backend CRUD API endpoints (`POST`, `GET`, `DELETE` for `/api/scans`).
- Added Request/Response DTOs, custom exception middleware handling, service, and repository layer for Scan entity.
- Fixed EF Core InMemory database isolation issue by using a fixed db name `"OsintToolkit_InMemory"` during DI registration.
- Fixed API tests JSON serialization issue by passing `JsonStringEnumConverter` settings to test HTTP clients.
- Verified all 42 tests across Api, Core, and Infrastructure projects (all passed).

### Files Added
- `backend/src/OsintToolkit.Api/Contracts/Requests/CreateScanRequest.cs`
- `backend/src/OsintToolkit.Api/Contracts/Responses/ScanDetailResponse.cs`
- `backend/src/OsintToolkit.Api/Contracts/Responses/ScanResponse.cs`
- `backend/src/OsintToolkit.Api/Contracts/Responses/ScanResultResponse.cs`
- `backend/src/OsintToolkit.Api/Controllers/ScanController.cs`
- `backend/src/OsintToolkit.Core/Exceptions/NotFoundException.cs`
- `backend/src/OsintToolkit.Core/Exceptions/ValidationException.cs`
- `backend/src/OsintToolkit.Core/Interfaces/IScanRepository.cs`
- `backend/src/OsintToolkit.Core/Interfaces/IScanService.cs`
- `backend/src/OsintToolkit.Core/Services/ScanService.cs`
- `backend/src/OsintToolkit.Infrastructure/Repositories/ScanRepository.cs`
- `backend/tests/OsintToolkit.Api.Tests/ScanEndpointTests.cs`
- `backend/tests/OsintToolkit.Core.Tests/ScanServiceTests.cs`
- `backend/tests/OsintToolkit.Infrastructure.Tests/ScanRepositoryTests.cs`
- `frontend/src/api/scans.ts`
- `frontend/src/types/scans.ts`

### Files Modified
- `backend/src/OsintToolkit.Api/Middleware/ExceptionHandlingMiddleware.cs`
- `backend/src/OsintToolkit.Api/Program.cs`
- `backend/src/OsintToolkit.Core/Services/TargetClassifier.cs`
- `backend/src/OsintToolkit.Infrastructure/DependencyInjection.cs`
- `backend/src/OsintToolkit.Infrastructure/OsintToolkit.Infrastructure.csproj`
- `backend/tests/OsintToolkit.Api.Tests/OsintToolkit.Api.Tests.csproj`
- `backend/tests/OsintToolkit.Core.Tests/TargetClassifierTests.cs`
- `frontend/src/api/client.ts`
- `docs/progress-log.md`

### Commit
feat: implement backend scan management mvp and verify tests

### Next Task
- Implement frontend UI for Scan Management (New Scan Form, Scan History, Scan Detail).
- Start Sprint 2 - Real OSINT Modules.

## 2026-07-15

### Sprint
Sprint 1

### Progress
- Implemented state-based routing in the frontend `App.tsx` coordinating three primary views.
- Created `NewScan.tsx` form component containing target validations and dynamic module selections.
- Created `ScanDetail.tsx` page to display scan overview and expandable result card list containing raw JSON output.
- Rebuilt `Dashboard.tsx` to list scan histories, show system metrics, and support query filtering by status/type.
- Expanded `main.css` styling for cards, buttons, tables, badges, error states, and responsive layouts.
- Verified compilation and Vite production build (all succeeded).

### Files Added
- `frontend/src/pages/NewScan.tsx`
- `frontend/src/pages/ScanDetail.tsx`

### Files Modified
- `frontend/src/pages/Dashboard.tsx`
- `frontend/src/App.tsx`
- `frontend/src/styles/main.css`
- `docs/progress-log.md`

### Commit
feat: implement frontend UI for scan management MVP and custom styling

### Next Task
- Start Sprint 2 - Implement real OSINT modules (DNS Lookup, WHOIS, Email validation).

## 2026-09-09

### Sprint
Sprint 2

### Progress
- Verified Sprint 1 end-to-end (PostgreSQL, backend health, frontend, scan CRUD flow, CORS preflight).
- Designed a module abstraction in `OsintToolkit.Core`: `IOSINTModule`, `OSINTModuleResult`, and `ModuleRegistry`.
- Implemented `DnsLookupModule` with a dependency-free DNS-over-UDP resolver (`DnsResolver`) that queries A, AAAA, MX, TXT, NS, and reverse PTR records.
- Implemented `WhoisLookupModule` via the RDAP bootstrap service (`https://rdap.org/domain/...`). RDAP is the modern machine-readable successor to port-43 WHOIS and works in restricted networks where outbound TCP 43 is blocked. Set a custom User-Agent and `Accept: application/rdap+json` because Verisign's RDAP endpoint rejects the .NET default user agent.
- Implemented `EmailValidationModule`: format check plus MX record lookup of the mail domain (no messages are ever sent).
- Refactored `ScanService` to resolve and run modules per scan, replacing Sprint 1 placeholder results. Unknown modules fail the result, recognized-but-unimplemented modules are marked `Skipped`, scan status becomes `Completed` unless any module failed.
- Verified the DNS resolver handles compressed names, null MX (`0 .`), TXT character strings, and reverse names.
- Wrote tests: `ModuleRegistryTests` plus updated `ScanServiceTests` to inject a fake module resolver (no network in unit tests).
- Ran `dotnet test -m:1`: 59 tests passed (API 7, Core 48, Infrastructure 4).
- Verified end-to-end via the API for `google.com` (DnsLookup Completed, WhoisLookup Completed with registrar/expiry), `analyst@gmail.com` (EmailValidation Completed, 5 MX), `8.8.8.8` (DnsLookup PTR `dns.google`, IpReputation Skipped), and a username (UsernameChecker Skipped).
- Cleaned up verification scan data.
- Bumped milestone in README to v0.2.0-alpha / Sprint 2 and documented the module status table.

### Files Added
- `backend/src/OsintToolkit.Core/Modules/IOSINTModule.cs`
- `backend/src/OsintToolkit.Core/Modules/OSINTModuleResult.cs`
- `backend/src/OsintToolkit.Core/Modules/ModuleRegistry.cs`
- `backend/src/OsintToolkit.Core/Modules/DnsResolver.cs`
- `backend/src/OsintToolkit.Core/Modules/DnsLookupModule.cs`
- `backend/src/OsintToolkit.Core/Modules/WhoisLookupModule.cs`
- `backend/src/OsintToolkit.Core/Modules/EmailValidationModule.cs`
- `backend/tests/OsintToolkit.Core.Tests/ModuleRegistryTests.cs`

### Files Modified
- `backend/src/OsintToolkit.Core/Services/ScanService.cs`
- `backend/tests/OsintToolkit.Core.Tests/ScanServiceTests.cs`
- `README.md`
- `docs/progress-log.md`

### Commit
(not committed)

### Next Task
- Implement Sprint 3 modules: UsernameChecker (platform presence) and IpReputation (threat intelligence feed).
- Revisit traditional port-43 WHOIS as an optional fallback behind RDAP.

## 2026-09-10

### Sprint
Sprint 3

### Progress
- Implemented `UsernameCheckerModule`: checks public profile existence on GitHub (`/users/{username}`), GitLab (`/users?username=`), and HackerNews (Firebase v0 API) using unauthenticated JSON endpoints. Captures profile metadata (name, followers, repos, karma, web URL) for found platforms.
- Implemented `IpReputationModule`: enriches an IP with public ipinfo.io data (hostname, org, geolocation, anycast) and derives a heuristic risk score (0-100) from Tor exit/proxy/VPN/abuse hostname and org signals. No API key required.
- Added `RiskScore` to `OSINTModuleResult` and plumbed the highest module risk into the scan's `RiskScore` so the Dashboard and Scan Detail can show `x/100`.
- Registered both modules in `ModuleRegistry`; removed the recognized-but-unimplemented placeholders.
- Verified end-to-end via API:
  - `torvalds` -> UsernameChecker Completed, found on GitHub (1 of 3 platforms).
  - nonexistent username -> Completed, not found on any platform.
  - `8.8.8.8` -> IpReputation Completed, risk 5/100 (anycast).
  - `185.220.101.4` (Tor exit) -> IpReputation Completed, risk 55/100 (tor/exit signals).
- Ran `dotnet test -m:1`: 62 tests passed (API 7, Core 51, Infrastructure 4).
- Updated README to v0.3.0-alpha / Sprint 3, frontend labels, and cleaned up verification data.

### Files Added
- `backend/src/OsintToolkit.Core/Modules/UsernameCheckerModule.cs`
- `backend/src/OsintToolkit.Core/Modules/IpReputationModule.cs`

### Files Modified
- `backend/src/OsintToolkit.Core/Modules/OSINTModuleResult.cs`
- `backend/src/OsintToolkit.Core/Modules/ModuleRegistry.cs`
- `backend/src/OsintToolkit.Core/Services/ScanService.cs`
- `backend/tests/OsintToolkit.Core.Tests/ModuleRegistryTests.cs`
- `backend/tests/OsintToolkit.Core.Tests/ScanServiceTests.cs`
- `README.md`
- `frontend/src/App.tsx`
- `frontend/src/pages/NewScan.tsx`
- `docs/progress-log.md`

### Commit
(not committed)

### Next Task
- Sprint 4: Report PDF generation from scan results.
- Optional: add abuseipdb/VirusTotal API-key-backed reputation feed behind the public-data heuristic.

## 2026-09-10 (Sprint 4 - Report PDF)

### Progress
- Added PdfSharp 6.2.4 to the Infrastructure project and implemented `PdfReportGenerator` (`IReportPdfGenerator`): A4 portrait PDF with scan header, metadata (target, status, risk score, timestamps, scan ID, notes), and one section per module result including pretty-printed raw JSON. Auto page breaks, soft line wrapping, and a `FontResolver` mapping the base-14 Helvetica/Courier names to system DejaVu fonts (PdfSharp 6 no longer bundles base-14 fonts).
- Added `ReportService` plus `ReportRepository` and registered report dependencies in DI. `ReportService` writes the PDF to `Report:OutputDirectory` (default `reports/generated/`), normalizes custom file names to `.pdf`, and persists a `Report` row.
- Added `ReportController` with three endpoints: `POST /api/scans/{scanId}/reports` (201 + report), `GET /api/scans/{scanId}/reports` (list), `GET /api/reports/{reportId}/download` (application/pdf, attachment filename).
- Added frontend report UI in the Scan Detail page: Generate Report button, report list with generated-at timestamps, and Download links that open the PDF.
- Verified end-to-end via API: created a DNS scan, generated `e2e-report.pdf` (52.2 KB, `%PDF-1.7`), confirmed it is listed and downloadable with `application/pdf`, and that the file lands in `reports/generated/`.
- Ran `dotnet test -m:1`: 71 tests passed (API 11, Core 56, Infrastructure 4).
- Updated README to v0.4.0-alpha / Sprint 4.

### Files Added
- `backend/src/OsintToolkit.Core/Interfaces/IReportRepository.cs`
- `backend/src/OsintToolkit.Core/Interfaces/IReportPdfGenerator.cs`
- `backend/src/OsintToolkit.Core/Interfaces/IReportService.cs`
- `backend/src/OsintToolkit.Core/Services/ReportService.cs`
- `backend/src/OsintToolkit.Infrastructure/Repositories/ReportRepository.cs`
- `backend/src/OsintToolkit.Infrastructure/Services/PdfReportGenerator.cs`
- `backend/src/OsintToolkit.Infrastructure/Services/FontResolver.cs`
- `backend/src/OsintToolkit.Api/Controllers/ReportController.cs`
- `backend/src/OsintToolkit.Api/Contracts/Requests/GenerateReportRequest.cs`
- `backend/src/OsintToolkit.Api/Contracts/Responses/ReportResponse.cs`
- `backend/tests/OsintToolkit.Core.Tests/ReportServiceTests.cs`
- `backend/tests/OsintToolkit.Api.Tests/ReportEndpointTests.cs`
- `frontend/src/api/reports.ts`
- `frontend/src/types/reports.ts`

### Files Modified
- `backend/src/OsintToolkit.Infrastructure/OsintToolkit.Infrastructure.csproj`
- `backend/src/OsintToolkit.Infrastructure/DependencyInjection.cs`
- `backend/src/OsintToolkit.Api/appsettings.json`
- `frontend/src/pages/ScanDetail.tsx`
- `frontend/src/styles/main.css`
- `README.md`
- `docs/progress-log.md`

### Commit
(not committed)

### Next Task
- Sprint 5: Python worker (e.g. a small companion script/service that post-processes exported reports or modules).
- Optional: add abuseipdb/VirusTotal API-key-backed reputation feed behind the public-data heuristic.

## 2026-09-10 (Sprint 5 - Passive Subdomain Finder / Scan Notes)

### Progress
- Implemented `SubdomainFinderModule`: passive subdomain enumeration from public certificate transparency logs via `crt.sh/?q=%25.{domain}&output=json`. `ParseCertificateData` splits newline-delimited `name_value` fields, filters wildcard entries and names outside the target domain, and returns a sorted de-duplicated list (first 500 stored in raw data, full count in summary). Fully passive; never contacts the target.
- Registered `SubdomainFinder` in `ModuleRegistry` (Modules, KnownIds, `SupportedFor(Domain)`).
- Added editable scan notes: `PATCH /api/scans/{scanId}/notes` -> `ScanService.UpdateNotesAsync` (trims, blank clears to null). Default scan notes are now `null` instead of the stale hard-coded sprint label.
- Frontend: added the Subdomain Finder toggle to the Domain module list in New Scan, and an inline notes editor (Edit/Save/Cancel) in the Scan Detail Overview panel.
- Verified end-to-end via API:
  - `cloudflare.com` + SubdomainFinder -> Completed, "Found 3422 unique subdomain(s) ... via certificate transparency".
  - `PATCH .../notes` with `{"notes":"review SSL certs from CT logs"}` -> 200, notes persisted.
- Ran `dotnet test -m:1`: 80 tests passed (API 13, Core 63, Infrastructure 4).
- Updated README to v0.5.0-alpha / Sprint 5.

### Files Added
- `backend/src/OsintToolkit.Core/Modules/SubdomainFinderModule.cs`
- `backend/src/OsintToolkit.Api/Contracts/Requests/UpdateScanNotesRequest.cs`

### Files Modified
- `backend/src/OsintToolkit.Core/Modules/ModuleRegistry.cs`
- `backend/src/OsintToolkit.Core/Interfaces/IScanService.cs`
- `backend/src/OsintToolkit.Core/Services/ScanService.cs`
- `backend/src/OsintToolkit.Api/Controllers/ScanController.cs`
- `backend/tests/OsintToolkit.Core.Tests/ModuleRegistryTests.cs`
- `backend/tests/OsintToolkit.Core.Tests/ScanServiceTests.cs`
- `backend/tests/OsintToolkit.Api.Tests/ScanEndpointTests.cs`
- `frontend/src/api/client.ts`
- `frontend/src/api/scans.ts`
- `frontend/src/types/scans.ts`
- `frontend/src/pages/NewScan.tsx`
- `frontend/src/pages/ScanDetail.tsx`
- `frontend/src/styles/main.css`
- `README.md`
- `docs/progress-log.md`

### Commit
(not committed)

### Next Task
- Optional: abuseipdb/VirusTotal API-key-backed reputation feed behind the public-data heuristic.
- Optional: verify subdomain findings (e.g. resolve the enumerated names) or export a subdomain list.
- Python worker remains on hold per ADR-001 until a specific Python OSINT library justifies it.

## 2026-09-10 (Sprint 5b - Subdomain Verification / JSON Export)

### Progress
- Implemented `SubdomainResolveModule`: re-fetches the passive crt.sh candidates (`FetchCertificateDataAsync`, shared with `SubdomainFinderModule`) and resolves up to 25 of them to A/AAAA records using the built-in `DnsResolver`. No wordlist/brute force: only names already attested in public certificate transparency logs are resolved.
- Hardened the crt.sh fetch: both CT modules now retry once after 1.5 s on transient 5xx (crt.sh is occasionally flaky, observed a one-off 502/404).
- Registered `SubdomainResolve` in `ModuleRegistry` (Modules, KnownIds, `SupportedFor(Domain)`).
- Added `GET /api/scans/{scanId}/export`: returns the full scan detail (metadata + module results) as a downloadable formatted JSON file (`Content-Disposition: attachment`), serialized camelCase with relaxed escaping so raw JSON stays readable and consistent with the API.
- Frontend: added the Subdomain Verification toggle in New Scan, and an Export JSON button (download link) in the Scan Detail header.
- Verified end-to-end via API:
  - `cloudflare.com` + SubdomainResolve -> Completed, "2 of 25 passive subdomain candidate(s) resolved to IP addresses"; `ajax.cloudflare.com`/`cdnjs.cloudflare.com` resolved to public IPs, historical `ssl*` names correctly returned no records.
  - `GET /api/scans/{id}/export` -> HTTP 200, `application/json`, `Content-Disposition: filename=scan-cloudflare.com.json`, camelCase body containing module results.
- Ran `dotnet test -m:1`: 82 tests passed (API 14, Core 64, Infrastructure 4).
- Updated README to v0.5.1-alpha / Sprint 5b.

### Files Added
- `backend/src/OsintToolkit.Core/Modules/SubdomainResolveModule.cs`

### Files Modified
- `backend/src/OsintToolkit.Core/Modules/SubdomainFinderModule.cs` (shared retry helper)
- `backend/src/OsintToolkit.Core/Modules/ModuleRegistry.cs`
- `backend/src/OsintToolkit.Api/Controllers/ScanController.cs` (export endpoint)
- `backend/tests/OsintToolkit.Core.Tests/ModuleRegistryTests.cs`
- `backend/tests/OsintToolkit.Api.Tests/ScanEndpointTests.cs`
- `frontend/src/pages/NewScan.tsx`
- `frontend/src/api/scans.ts`
- `frontend/src/pages/ScanDetail.tsx`
- `README.md`
- `docs/progress-log.md`

### Commit
(not committed)

### Next Task
- Optional: abuseipdb/VirusTotal API-key-backed reputation feed behind the public-data heuristic.
- Optional: export CSV of resolved subdomains, or batch re-verify on demand.
- Python worker remains on hold per ADR-001 until a specific Python OSINT library justifies it.


