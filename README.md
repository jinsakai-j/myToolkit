# OSINT Toolkit Local

![.NET 9](https://img.shields.io/badge/.NET-9-512BD4)
![React](https://img.shields.io/badge/React-18-61DAFB)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1)
![License](https://img.shields.io/badge/License-MIT-green)
![Project Status](https://img.shields.io/badge/Status-Sprint%203%20Username%20Checker%20%2F%20IP%20Reputation-yellow)

Local OSINT dashboard project for a cybersecurity and software engineering portfolio.

The project is currently in **Sprint 3 - Username Checker / IP Reputation**. OSINT scan management (Sprint 1) and four real modules (DNS, WHOIS/RDAP, email validation, username checker) are complete; IP reputation now enriches lookup data with a heuristic risk score.

## Project Status

Current Version:

```text
v0.3.0-alpha
```

Current Sprint:

```text
Sprint 3
```

Current Status:

```text
Sprint 3 - Username Checker / IP Reputation
```

Next Milestone:

```text
Sprint 4 - Report PDF
```

## Tech Stack

- Frontend: React + Vite + TypeScript
- Backend: ASP.NET Core Web API (.NET 9)
- Database: PostgreSQL
- ORM: Entity Framework Core
- Testing: xUnit

## Ethical Scope

This project is limited to legal and ethical OSINT workflows:

- Public data only
- No brute force
- No login bypass
- No exploit attempts
- No aggressive scanning
- No unauthorized access

## Prerequisites

- .NET 9 SDK
- Node.js 20+
- npm
- Docker and Docker Compose

## Run Locally

### 1. Start PostgreSQL

Copy `.env.example` to `.env` if you want to override the default local Docker Compose values.

```bash
docker compose up -d postgres
```

The project PostgreSQL container maps to host port `5433` to avoid collisions with a local PostgreSQL service on `5432`.

### 2. Restore and migrate backend

```bash
cd backend
dotnet restore
dotnet tool restore
dotnet ef database update \
  --project src/OsintToolkit.Infrastructure \
  --startup-project src/OsintToolkit.Api
```

### 3. Run backend API

```bash
cd backend
dotnet run --project src/OsintToolkit.Api
```

Backend default URL:

```text
http://localhost:5080
```

Swagger UI in development:

```text
http://localhost:5080/swagger
```

Health endpoint:

```text
GET http://localhost:5080/api/health
```

### 4. Run frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend default URL:

```text
http://localhost:5173
```

### 5. Run tests

```bash
cd backend
dotnet test
```

If the test runner hits local socket restrictions in a sandboxed environment, run:

```bash
dotnet test -m:1
```

## Sprint 0 Status

Implemented in Sprint 0:

- Backend solution structure
- API, Core, Infrastructure, and test projects
- Swagger/OpenAPI setup
- CORS setup for frontend localhost
- `GET /api/health`
- PostgreSQL Docker Compose service
- EF Core DbContext
- Initial entities: `Scan`, `ScanResult`, `Report`
- Initial migration source files
- React + Vite + TypeScript frontend shell
- Frontend health check call
- `.env.example`

Implemented in Sprint 1:

- Scan CRUD API (`POST`, `GET`, `DELETE` `/api/scans`)
- React frontend for New Scan, Scan History, and Scan Detail

Implemented in Sprint 2 - Real OSINT modules:

- DNS lookup (A, AAAA, MX, TXT, NS, reverse PTR) via a built-in DNS over UDP resolver
- WHOIS lookup via the RDAP bootstrap service (machine-readable successor to port 43 WHOIS)
- Email validation (format + MX record check)

Implemented in Sprint 3 - more OSINT modules:

- Username checker (GitHub, GitLab, HackerNews public profile lookup)
- IP reputation (ipinfo.io enrichment plus heuristic risk signals from Tor/proxy/VPN host hints, with scan risk score)

Module ids used by the frontend:

```text
DnsLookup       -> Domain, IpAddress (reverse)
WhoisLookup     -> Domain (RDAP)
EmailValidation -> Email
UsernameChecker -> Username (GitHub, GitLab, HackerNews)
IpReputation    -> IpAddress (ipinfo.io + heuristics, RiskScore 0-100)
```

Not implemented yet:

- Report PDF
- Python worker

## Repository Hygiene

- Do not commit `.env`, `appsettings.Local.json`, build outputs, local SDK folders, `node_modules`, frontend `dist`, coverage reports, logs, or generated PDF reports.
- Keep `.env.example` committed with placeholder local development values only.
- Local generated folders do not need to be deleted if they were never tracked by Git and are already covered by `.gitignore`.
- Update `docs/progress-log.md` after each meaningful task.

## Project Documentation

- [Project Plan](docs/project-plan.md)
- [Architecture](docs/architecture.md)
- [Ethical Guidelines](docs/ethical-guidelines.md)
- [Progress Log](docs/progress-log.md)
