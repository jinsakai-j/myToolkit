# OSINT Toolkit Local - Project Plan

## 1. Project Overview

**OSINT Toolkit Local** adalah aplikasi dashboard lokal untuk membantu analisis OSINT legal dan etis terhadap data publik. Aplikasi berjalan di `localhost`, menyimpan hasil ke database lokal PostgreSQL, dan ditujukan sebagai portfolio **cybersecurity + software engineering**.

Target input awal:

- Domain
- Email
- Username
- IP address

Output utama:

- Dashboard hasil analisis
- Riwayat scan
- Detail hasil per target
- Export report PDF

Batasan utama:

- Hanya mengambil data publik.
- Tidak brute force.
- Tidak bypass login.
- Tidak exploit target.
- Tidak scanning agresif.
- Tidak mengambil data dari area privat atau autentikasi.

## 2. Review Planning Sebelumnya

Planning sebelumnya sudah kuat di sisi konsep OSINT, batasan etis, roadmap, dan daftar fitur. Namun perlu disesuaikan sebelum coding karena:

- Stack lama masih mengarah ke **FastAPI + SQLite**, sedangkan stack final adalah **ASP.NET Core Web API (.NET 9) + PostgreSQL + EF Core**.
- Endpoint terlalu banyak dan terlalu spesifik untuk MVP. Untuk awal, lebih baik pakai endpoint scan generik agar arsitektur tidak cepat melebar.
- Backend perlu dipisah menjadi layer `Api`, `Core`, `Infrastructure`, dan `Tests`.
- Sprint 0 harus fokus ke foundation: solution structure, database connection, health check, CI-ready test project, dan dokumentasi.
- Fitur OSINT belum boleh diimplementasikan pada Sprint 0.
- Python worker sebaiknya tetap opsional, bukan fondasi utama.

Keputusan revisi:

- Gunakan `.NET 9` sebagai backend utama.
- Gunakan `React + Vite + TypeScript` untuk frontend.
- Gunakan `PostgreSQL` untuk database lokal.
- Gunakan `Entity Framework Core` untuk ORM dan migration.
- Gunakan `xUnit` untuk testing backend.
- Gunakan endpoint MVP yang sedikit, stabil, dan scan-centric.

## 3. Final Tech Stack

### Frontend

```text
React + Vite + TypeScript
```

Alasan:

- Modern dan portfolio-friendly.
- TypeScript menunjukkan engineering discipline.
- Cocok untuk dashboard interaktif.
- Vite ringan untuk development lokal.

### Backend

```text
ASP.NET Core Web API (.NET 9)
```

Alasan:

- Cocok untuk API terstruktur.
- Strong typing bagus untuk portfolio software engineering.
- Mudah menerapkan layered architecture.
- Cocok dengan EF Core, PostgreSQL, Swagger/OpenAPI, dan xUnit.

### Database

```text
PostgreSQL
```

Alasan:

- Lebih production-like dibanding SQLite.
- Bagus untuk portfolio karena menunjukkan skill relational database.
- Mendukung JSONB untuk menyimpan raw result OSINT secara fleksibel.

### ORM

```text
Entity Framework Core
```

Alasan:

- Migration terkelola.
- Query typed dan maintainable.
- Cocok dengan repository/service pattern sederhana.

### Testing

```text
xUnit
```

Alasan:

- Standard testing framework di ekosistem .NET.
- Cocok untuk unit test domain logic, service, validator, dan API integration test.

### Optional Worker

```text
Python worker
```

Python worker hanya dipakai nanti jika ada library OSINT yang jauh lebih matang di Python. Untuk Sprint 0 sampai MVP awal, backend utama tetap `.NET`.

## 4. MVP Scope yang Direvisi

MVP tidak perlu langsung menjalankan semua modul OSINT. MVP yang realistis adalah membangun alur scan end-to-end terlebih dahulu.

### MVP Target

User bisa:

1. Membuka dashboard lokal.
2. Membuat scan baru dengan target dan tipe target.
3. Melihat daftar scan.
4. Melihat detail scan.
5. Melihat placeholder result per modul.
6. Generate report PDF dari data scan.

### MVP Feature Set

Fitur awal:

- Local dashboard
- Create scan
- Scan history
- Scan detail
- PostgreSQL persistence
- Basic validation target type
- Placeholder scan result
- Report metadata
- PDF generation sederhana pada fase MVP berikutnya

Fitur OSINT nyata ditunda sampai foundation stabil:

- DNS lookup
- WHOIS lookup
- Subdomain finder
- Email validation
- Username checker
- IP reputation lookup

## 5. Prioritas Fitur OSINT

Fitur tetap dipertahankan sebagai arah produk, tetapi implementasinya bertahap.

| Priority | Feature | Mode Aman | API Key |
| --- | --- | --- | --- |
| P1 | DNS Lookup | Query DNS record publik | Tidak wajib |
| P1 | Email Validation | Format + domain + MX check | Tidak wajib |
| P1 | Report PDF | Generate lokal dari database | Tidak wajib |
| P2 | WHOIS Lookup | Query WHOIS publik | Tidak wajib, tetapi bisa rate limited |
| P2 | IP Basic Lookup | Validasi IP + reverse DNS | Tidak wajib |
| P3 | Passive Subdomain Finder | Certificate Transparency/public source | Opsional |
| P3 | Username Checker | Public profile check dengan rate limit | Sebaiknya API resmi |
| P4 | IP Reputation | Threat intel provider | Ya |

## 6. Struktur Folder Final

```text
OSINT-Toolkit/
├── backend/
│   ├── OsintToolkit.sln
│   ├── src/
│   │   ├── OsintToolkit.Api/
│   │   │   ├── Controllers/
│   │   │   ├── Contracts/
│   │   │   │   ├── Requests/
│   │   │   │   └── Responses/
│   │   │   ├── Middleware/
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   └── appsettings.Development.json
│   │   │
│   │   ├── OsintToolkit.Core/
│   │   │   ├── Entities/
│   │   │   ├── Enums/
│   │   │   ├── Interfaces/
│   │   │   ├── Services/
│   │   │   ├── ValueObjects/
│   │   │   └── Exceptions/
│   │   │
│   │   └── OsintToolkit.Infrastructure/
│   │       ├── Data/
│   │       │   ├── AppDbContext.cs
│   │       │   ├── Configurations/
│   │       │   └── Migrations/
│   │       ├── Repositories/
│   │       ├── ReportGeneration/
│   │       ├── Osint/
│   │       │   ├── Dns/
│   │       │   ├── Whois/
│   │       │   ├── Email/
│   │       │   ├── Username/
│   │       │   └── Ip/
│   │       └── DependencyInjection.cs
│   │
│   └── tests/
│       ├── OsintToolkit.Api.Tests/
│       ├── OsintToolkit.Core.Tests/
│       └── OsintToolkit.Infrastructure.Tests/
│
├── frontend/
│   ├── src/
│   │   ├── api/
│   │   ├── components/
│   │   ├── features/
│   │   │   ├── scans/
│   │   │   ├── reports/
│   │   │   └── settings/
│   │   ├── pages/
│   │   ├── routes/
│   │   ├── styles/
│   │   ├── types/
│   │   ├── App.tsx
│   │   └── main.tsx
│   ├── package.json
│   ├── tsconfig.json
│   └── vite.config.ts
│
├── docs/
│   ├── project-plan.md
│   ├── architecture.md
│   ├── ethical-guidelines.md
│   ├── api-reference.md
│   └── sprint-checklists.md
│
├── reports/
│   └── generated/
│
├── docker/
│   └── postgres/
│
├── .env.example
├── .gitignore
├── docker-compose.yml
└── README.md
```

## 7. Backend Architecture

Backend memakai arsitektur sederhana berbasis layer:

```text
OsintToolkit.Api
  -> OsintToolkit.Core
  -> OsintToolkit.Infrastructure
```

### 7.1 API Layer

Project:

```text
OsintToolkit.Api
```

Tanggung jawab:

- HTTP endpoints
- Request/response DTO
- Input validation ringan
- Swagger/OpenAPI
- Error handling middleware
- Dependency injection bootstrapping
- CORS untuk frontend lokal

Tidak boleh berisi:

- Query EF Core langsung dari controller
- Logic OSINT detail
- Business rule utama

### 7.2 Core Layer

Project:

```text
OsintToolkit.Core
```

Tanggung jawab:

- Entity domain
- Enum domain
- Interface service/repository
- Business rule dasar
- Target classification
- Scan orchestration contract
- Validation rule yang tidak bergantung framework

Tidak boleh berisi:

- EF Core dependency
- PostgreSQL-specific code
- HTTP-specific code
- File system implementation detail

### 7.3 Infrastructure Layer

Project:

```text
OsintToolkit.Infrastructure
```

Tanggung jawab:

- EF Core `DbContext`
- PostgreSQL provider configuration
- Repository implementation
- Migration
- Report generator implementation
- OSINT provider implementation
- External HTTP client implementation
- Optional API key provider

### 7.4 Tests

Projects:

```text
OsintToolkit.Core.Tests
OsintToolkit.Infrastructure.Tests
OsintToolkit.Api.Tests
```

Fokus test:

- Core unit tests untuk target validation dan scan flow.
- Infrastructure tests untuk repository dan EF mapping.
- API tests untuk endpoint MVP.

## 8. Database Schema Awal

Database: PostgreSQL.

Gunakan EF Core migration, bukan SQL manual sebagai sumber utama. SQL di bawah ini adalah desain konseptual.

### `scans`

```sql
CREATE TABLE scans (
    id UUID PRIMARY KEY,
    target TEXT NOT NULL,
    target_type TEXT NOT NULL,
    status TEXT NOT NULL,
    risk_score INTEGER,
    created_at TIMESTAMPTZ NOT NULL,
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    notes TEXT
);
```

### `scan_results`

```sql
CREATE TABLE scan_results (
    id UUID PRIMARY KEY,
    scan_id UUID NOT NULL REFERENCES scans(id) ON DELETE CASCADE,
    module_name TEXT NOT NULL,
    status TEXT NOT NULL,
    summary TEXT,
    raw_data JSONB,
    created_at TIMESTAMPTZ NOT NULL
);
```

### `reports`

```sql
CREATE TABLE reports (
    id UUID PRIMARY KEY,
    scan_id UUID NOT NULL REFERENCES scans(id) ON DELETE CASCADE,
    file_name TEXT NOT NULL,
    file_path TEXT NOT NULL,
    generated_at TIMESTAMPTZ NOT NULL
);
```

### Enum Konseptual

`TargetType`:

- `Domain`
- `Email`
- `Username`
- `IpAddress`

`ScanStatus`:

- `Pending`
- `Running`
- `Completed`
- `Failed`
- `Cancelled`

`ModuleStatus`:

- `Pending`
- `Skipped`
- `Completed`
- `Failed`

## 9. Endpoint MVP Sederhana

Untuk MVP, jangan membuat endpoint terpisah seperti `/api/domain/dns`, `/api/domain/whois`, dan sejenisnya. Semua proses dimulai dari scan.

### Health

```http
GET /api/health
```

Tujuan:

- Memastikan backend hidup.
- Dipakai frontend untuk status check.

### Create Scan

```http
POST /api/scans
```

Request:

```json
{
  "target": "example.com",
  "targetType": "Domain",
  "modules": ["DnsLookup", "WhoisLookup"]
}
```

Catatan MVP:

- Pada Sprint 1, `modules` boleh disimpan sebagai rencana scan.
- Implementasi OSINT real belum wajib.
- Jika modul belum tersedia, hasil bisa dibuat sebagai `Skipped` atau placeholder.

### List Scans

```http
GET /api/scans
```

Query opsional:

```http
GET /api/scans?targetType=Domain&status=Completed
```

### Get Scan Detail

```http
GET /api/scans/{scanId}
```

### Delete Scan

```http
DELETE /api/scans/{scanId}
```

### Generate Report

```http
POST /api/scans/{scanId}/report
```

### Download Report

```http
GET /api/reports/{reportId}/download
```

Endpoint tambahan baru dibuat ketika ada kebutuhan nyata. Prinsip MVP: scan-centric API dulu.

## 10. User Flow MVP

```text
User
  -> buka React dashboard di localhost
  -> klik New Scan
  -> pilih target type
  -> isi target
  -> pilih modul yang direncanakan
  -> submit
  -> frontend call POST /api/scans
  -> backend validasi input
  -> backend simpan scan ke PostgreSQL
  -> backend simpan placeholder module result jika modul belum tersedia
  -> frontend redirect ke scan detail
  -> user melihat hasil
  -> user generate report
  -> user download PDF
```

## 11. Sprint 0 Checklist - Project Foundation

Sprint 0 fokus ke fondasi teknis. Tidak ada implementasi OSINT real.

### Repository & Documentation

- [ ] Buat `README.md` awal.
- [ ] Finalisasi `docs/project-plan.md`.
- [ ] Buat `docs/ethical-guidelines.md`.
- [ ] Buat `.gitignore`.
- [ ] Buat `.env.example`.
- [ ] Tentukan cara menjalankan backend, frontend, dan database lokal.

### Backend Foundation

- [ ] Buat .NET solution `OsintToolkit.sln`.
- [ ] Buat project `OsintToolkit.Api`.
- [ ] Buat project `OsintToolkit.Core`.
- [ ] Buat project `OsintToolkit.Infrastructure`.
- [ ] Hubungkan project references sesuai layering.
- [ ] Setup Swagger/OpenAPI.
- [ ] Setup CORS untuk frontend lokal.
- [ ] Buat endpoint `GET /api/health`.
- [ ] Buat global error handling middleware minimal.

### Database Foundation

- [ ] Setup PostgreSQL lokal via Docker Compose.
- [ ] Tambahkan connection string development.
- [ ] Setup EF Core di Infrastructure.
- [ ] Buat `AppDbContext`.
- [ ] Buat entity awal: `Scan`, `ScanResult`, `Report`.
- [ ] Buat EF Core migration pertama.
- [ ] Verifikasi migration bisa apply ke PostgreSQL lokal.

### Testing Foundation

- [ ] Buat `OsintToolkit.Core.Tests`.
- [ ] Buat `OsintToolkit.Infrastructure.Tests`.
- [ ] Buat `OsintToolkit.Api.Tests`.
- [ ] Tambahkan xUnit.
- [ ] Tambahkan test sederhana untuk health endpoint atau core validation.
- [ ] Pastikan `dotnet test` berjalan.

### Frontend Foundation

- [ ] Buat React app dengan Vite + TypeScript.
- [ ] Setup struktur folder frontend.
- [ ] Buat layout dasar dashboard.
- [ ] Buat halaman `Dashboard`.
- [ ] Buat halaman `NewScan` placeholder.
- [ ] Buat API client dasar.
- [ ] Call `GET /api/health` dari frontend.

### Definition of Done Sprint 0

- [ ] Backend berjalan lokal.
- [ ] Frontend berjalan lokal.
- [ ] PostgreSQL berjalan lokal.
- [ ] EF Core migration berhasil.
- [ ] Health check API bisa dipanggil.
- [ ] Frontend bisa membaca status backend.
- [ ] `dotnet test` berhasil.
- [ ] Tidak ada fitur OSINT real yang diimplementasikan.

## 12. Sprint 1 Checklist - Scan Management MVP

Sprint 1 fokus ke CRUD scan dan data model, bukan lookup OSINT real.

### Backend

- [ ] Buat DTO request/response scan.
- [ ] Buat enum `TargetType`.
- [ ] Buat enum `ScanStatus`.
- [ ] Buat enum `ModuleStatus`.
- [ ] Buat service `ScanService`.
- [ ] Buat repository interface di Core.
- [ ] Buat repository implementation di Infrastructure.
- [ ] Implement `POST /api/scans`.
- [ ] Implement `GET /api/scans`.
- [ ] Implement `GET /api/scans/{scanId}`.
- [ ] Implement `DELETE /api/scans/{scanId}`.
- [ ] Simpan selected modules sebagai placeholder result.
- [ ] Tambahkan validasi target minimal berdasarkan `targetType`.

### Database

- [ ] Pastikan relasi `Scan` ke `ScanResult`.
- [ ] Pastikan delete scan menghapus scan results.
- [ ] Gunakan `JSONB` untuk `raw_data` di `ScanResult`.

### Frontend

- [ ] Buat form `New Scan`.
- [ ] Buat pilihan target type.
- [ ] Buat checklist modules.
- [ ] Buat halaman scan list.
- [ ] Buat halaman scan detail.
- [ ] Tambahkan loading state.
- [ ] Tambahkan error state.

### Tests

- [ ] Unit test target validation.
- [ ] Unit test create scan service.
- [ ] API test create scan.
- [ ] API test get scan detail.
- [ ] Repository test basic persistence.

### Definition of Done Sprint 1

- [ ] User bisa membuat scan.
- [ ] User bisa melihat daftar scan.
- [ ] User bisa membuka detail scan.
- [ ] Data tersimpan di PostgreSQL.
- [ ] Tidak ada lookup OSINT real dulu.
- [ ] Test utama berjalan.

## 13. GitHub Issues yang Harus Dibuat

### Sprint 0 Issues

1. `docs: finalize project plan for .NET and React stack`
2. `docs: add ethical guidelines for legal OSINT scope`
3. `chore: initialize .NET solution structure`
4. `chore: create API, Core, Infrastructure, and test projects`
5. `chore: configure PostgreSQL with Docker Compose`
6. `feat: add backend health check endpoint`
7. `chore: configure Swagger and CORS`
8. `chore: setup EF Core DbContext and initial entities`
9. `chore: create initial EF Core migration`
10. `test: setup xUnit test projects`
11. `chore: initialize React Vite TypeScript frontend`
12. `feat: add frontend dashboard shell`
13. `feat: connect frontend to backend health endpoint`
14. `docs: add local development setup instructions`

### Sprint 1 Issues

15. `feat: add scan domain entities and enums`
16. `feat: add scan request and response DTOs`
17. `feat: implement scan repository`
18. `feat: implement scan service`
19. `feat: implement create scan endpoint`
20. `feat: implement list scans endpoint`
21. `feat: implement scan detail endpoint`
22. `feat: implement delete scan endpoint`
23. `feat: add target validation rules`
24. `feat: add new scan form in frontend`
25. `feat: add scan history page`
26. `feat: add scan detail page`
27. `test: add scan service unit tests`
28. `test: add scan API tests`
29. `test: add repository persistence tests`

### Future Issues

30. `feat: implement DNS lookup module`
31. `feat: implement email validation module`
32. `feat: implement WHOIS lookup module`
33. `feat: implement basic IP lookup module`
34. `feat: implement PDF report generation`
35. `feat: implement passive subdomain discovery`
36. `feat: implement username checker with rate limiting`
37. `feat: add optional API key settings`
38. `feat: add IP reputation provider integration`
39. `docs: add screenshots and portfolio demo guide`

## 14. Milestones 0.1 sampai 1.0

### v0.1 - Project Foundation

Fokus:

- Solution structure
- Frontend scaffold
- PostgreSQL local setup
- Health check
- Test foundation

Deliverable:

- Backend, frontend, dan database berjalan lokal.

### v0.2 - Scan Management

Fokus:

- Create scan
- List scan
- Scan detail
- Delete scan
- Placeholder module results

Deliverable:

- User bisa membuat dan melihat riwayat scan tanpa lookup OSINT real.

### v0.3 - DNS Lookup

Fokus:

- DNS record lookup untuk domain.
- Record awal: A, AAAA, MX, NS, TXT.

Deliverable:

- Domain scan bisa menghasilkan DNS result publik.

### v0.4 - Email Validation

Fokus:

- Email format validation.
- Domain extraction.
- MX lookup.

Deliverable:

- Email target bisa dianalisis secara pasif.

### v0.5 - WHOIS Lookup

Fokus:

- WHOIS publik.
- Registrar, created date, expiry date, name server.

Deliverable:

- Domain scan punya enrichment WHOIS.

### v0.6 - Basic IP Lookup

Fokus:

- IP validation.
- Reverse DNS.
- Basic ASN/geolocation jika sumber aman tersedia.

Deliverable:

- IP target bisa dianalisis dengan data publik dasar.

### v0.7 - PDF Report

Fokus:

- Generate report dari scan detail.
- Download PDF.
- Tambahkan disclaimer legal/etis.

Deliverable:

- Report bisa dipakai sebagai portfolio artifact.

### v0.8 - Passive Subdomain Discovery

Fokus:

- Certificate Transparency atau sumber publik pasif.
- Deduplication.
- Rate limiting.

Deliverable:

- Domain recon lebih lengkap tanpa brute force.

### v0.9 - Username Checker

Fokus:

- Public profile checker terbatas.
- Rate limiting.
- Status `Found`, `NotFound`, `Unknown`.

Deliverable:

- Username target bisa dicek di platform publik yang aman.

### v1.0 - Portfolio Polish

Fokus:

- UI polish.
- Error handling.
- Empty/loading states.
- Test coverage dasar.
- README lengkap.
- Screenshot.
- Demo data.

Deliverable:

- Project siap ditampilkan sebagai portfolio cybersecurity + software engineering.

## 15. Ethical and Legal Guardrails

Project harus selalu berada dalam batas berikut:

- Hanya data publik.
- Tidak login ke akun orang lain.
- Tidak bypass authentication.
- Tidak brute force.
- Tidak password reset probing.
- Tidak exploit vulnerability.
- Tidak port scanning otomatis.
- Tidak directory brute forcing.
- Tidak scraping masif.
- Tidak mengabaikan Terms of Service.
- Tidak menyimpan data sensitif tanpa kebutuhan jelas.

### Guardrail per Fitur

DNS lookup:

- Aman selama hanya query DNS record publik.

WHOIS lookup:

- Aman selama membaca data WHOIS publik dan menghormati rate limit.

Subdomain finder:

- Gunakan passive source.
- Jangan gunakan wordlist brute force pada MVP.

Username checker:

- Platform terbatas.
- Rate limit wajib.
- Prefer API resmi.
- Jangan cek flow reset password.

IP reputation:

- Tampilkan sumber data.
- Hindari klaim final seperti `confirmed malicious`.
- Gunakan istilah `potential risk` atau `reported by source`.

Report:

- Wajib menyertakan tanggal scan, source, dan disclaimer.

Disclaimer report:

```text
This report is generated from publicly available information only.
The results are intended for educational, defensive, and authorized research purposes.
No authentication bypass, exploitation, brute force, or unauthorized access was performed.
Findings should be manually verified before being used for security decisions.
```

## 16. Final Recommendation

Stack final:

```text
Frontend: React + Vite + TypeScript
Backend: ASP.NET Core Web API (.NET 9)
Database: PostgreSQL
ORM: Entity Framework Core
Testing: xUnit
Optional Worker: Python only when needed
```

Urutan kerja yang direkomendasikan:

1. Selesaikan Sprint 0 foundation.
2. Lanjut Sprint 1 scan management.
3. Baru implementasi modul OSINT satu per satu.
4. Pastikan setiap modul punya batasan etis, rate limit, error handling, dan test.

Prinsip arsitektur:

- API tetap tipis.
- Business rule ada di Core.
- EF Core dan provider OSINT ada di Infrastructure.
- Endpoint tetap scan-centric sampai kebutuhan memaksa pemisahan.
- Python worker tidak dipakai sebelum ada alasan teknis yang jelas.
