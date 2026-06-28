# OSINT Toolkit Local - Project Plan

## 1. Ringkasan Konsep Project

**OSINT Toolkit Local** adalah aplikasi dashboard lokal untuk membantu proses investigasi OSINT yang legal dan etis terhadap data publik.

Project ini berjalan di `localhost` dan menerima input berupa:

- Domain
- Email
- Username
- IP address

Aplikasi akan menjalankan lookup sederhana, mengelompokkan hasil analisis, menyimpan riwayat pencarian, dan menghasilkan report PDF.

Fokus utama project:

- Cybersecurity portfolio
- Software engineering portfolio
- Demonstrasi integrasi backend, frontend, database, worker, dan report generator
- Tidak melakukan brute force, bypass login, exploit, scraping agresif, atau akses tidak sah

Contoh use case:

- Mengecek informasi publik dari sebuah domain
- Melihat DNS record
- Mengecek format dan MX record email
- Mengecek keberadaan username di platform publik tertentu
- Mengecek reputasi IP melalui provider OSINT
- Menghasilkan report investigasi ringan dalam bentuk PDF

## 2. MVP Paling Realistis

MVP sebaiknya dibuat sederhana tapi lengkap secara alur.

### MVP Scope

Input awal:

- Domain
- Email
- IP address

Fitur MVP:

- Dashboard sederhana
- Domain DNS lookup
- WHOIS lookup dasar
- Email format validation
- Email domain MX lookup
- IP geolocation sederhana
- Simpan hasil scan ke database lokal
- Halaman detail hasil scan
- Export report PDF sederhana

Fitur yang ditunda setelah MVP:

- Username checker multi-platform
- Subdomain finder lebih lengkap
- IP reputation dengan API eksternal
- Risk scoring otomatis
- Queue worker background
- Authentication user lokal

Tujuan MVP:

> User bisa memasukkan target, sistem menjalankan beberapa lookup legal berbasis data publik, hasil tampil di dashboard, lalu bisa diexport menjadi PDF.

## 3. Daftar Fitur Prioritas

### 3.1 Domain Recon

Mengumpulkan informasi dasar dari domain publik.

Data yang dikumpulkan:

- Domain name
- DNS records
- WHOIS information
- Registrar
- Creation date
- Expiration date
- Name servers
- MX records
- TXT records
- Basic HTTP metadata

Catatan etis:

- Aman selama hanya membaca data publik.
- Tidak melakukan scanning agresif.
- Tidak melakukan exploit atau bypass akses.

### 3.2 DNS Lookup

Melakukan lookup DNS record dari domain.

Record awal:

- A
- AAAA
- MX
- NS
- TXT
- CNAME
- SOA

Contoh output:

```text
A Record:
- 104.21.x.x
- 172.67.x.x

MX Record:
- mail.example.com
```

Aman tanpa API key karena bisa menggunakan DNS resolver lokal atau library DNS.

### 3.3 WHOIS Lookup

Mengambil informasi WHOIS domain.

Output:

- Registrar
- Created date
- Updated date
- Expiry date
- Name servers
- Status domain
- WHOIS raw text jika tersedia

Catatan:

- Beberapa domain memakai WHOIS privacy sehingga data pemilik bisa disembunyikan.
- Hasil WHOIS bisa tidak konsisten tergantung TLD dan limit WHOIS server.

### 3.4 Subdomain Finder Sederhana

Gunakan metode pasif, bukan brute force agresif.

Sumber aman:

- Certificate Transparency logs
- Public DNS sources
- API seperti `crt.sh`
- Dataset publik

Output:

- Subdomain
- Source
- Resolved IP jika tersedia
- Status aktif jika dicek dengan DNS resolve ringan

Hindari:

- Bruteforce ribuan wordlist
- Port scanning
- Directory scanning
- Crawling agresif

### 3.5 Email Validation

Validasi email secara pasif.

Pemeriksaan:

- Format email valid
- Domain email valid
- MX record tersedia
- Disposable email check opsional
- Domain reputation opsional

Yang tidak dilakukan:

- Tidak login ke mailbox
- Tidak mencoba SMTP enumeration agresif
- Tidak mengecek apakah inbox benar-benar aktif dengan metode intrusive

### 3.6 Username Checker

Mengecek apakah username muncul di beberapa platform publik.

Cara aman:

- Request halaman profil publik dengan rate limit.
- Gunakan daftar platform terbatas.
- Tampilkan status `found`, `not_found`, atau `unknown`.

Platform awal:

- GitHub
- GitLab
- Reddit
- Medium

Platform seperti Instagram atau X sebaiknya hanya dipertimbangkan jika akses publiknya stabil dan tidak melanggar Terms of Service.

Batasan:

- Tidak bypass login.
- Tidak scraping halaman yang melarang automation.
- Tidak mencoba password reset.
- Tidak mengumpulkan data sensitif.

### 3.7 IP Reputation Lookup

Mengecek reputasi IP dari sumber OSINT.

Data:

- Geolocation
- ASN
- ISP
- Country
- Abuse score
- Known malicious status
- Proxy/VPN/Tor indicator jika tersedia

Tanpa API key:

- IP format validation
- Reverse DNS
- Basic geolocation dari database lokal seperti GeoLite2
- ASN lookup jika memakai sumber lokal

Butuh API key untuk reputasi yang lebih kuat:

- AbuseIPDB
- VirusTotal
- GreyNoise
- Shodan
- SecurityTrails

### 3.8 Report Generator PDF

Membuat laporan hasil investigasi.

Isi report:

- Judul report
- Tanggal scan
- Target
- Jenis target
- Ringkasan hasil
- Detail hasil per modul
- Risk notes
- Sumber data
- Disclaimer legal/etis

Aman tanpa API key karena semua proses generate report bisa berjalan lokal.

## 4. Fitur Tanpa API Key vs Butuh API Key

### Aman Dibuat Tanpa API Key

| Fitur | Keterangan |
| --- | --- |
| DNS Lookup | Menggunakan resolver DNS |
| WHOIS Lookup | Bisa langsung query WHOIS server |
| Email format validation | Parser lokal |
| Email MX lookup | DNS query |
| Basic domain recon | DNS, WHOIS, HTTP metadata |
| Basic IP validation | Parsing lokal |
| Reverse DNS | DNS PTR lookup |
| Basic subdomain via crt.sh | Bisa tanpa key, tergantung availability |
| Report PDF | Generate lokal |
| History scan | Database lokal |
| Dashboard | Lokal |

### Sebaiknya Pakai API Key

| Fitur | Provider Contoh |
| --- | --- |
| IP reputation | AbuseIPDB, VirusTotal, GreyNoise |
| Domain reputation | VirusTotal, URLScan, SecurityTrails |
| Subdomain enrichment | SecurityTrails, Shodan, Censys |
| Username checker stabil | API resmi masing-masing platform |
| Disposable email database | AbstractAPI, Kickbox, Hunter |
| Threat intelligence enrichment | VirusTotal, AlienVault OTX |

Rekomendasi portfolio:

- Fitur dasar jalan tanpa API key.
- API key bersifat opsional.
- Jika API key tidak tersedia, tampilkan status `Skipped: API key not configured`.
- Simpan konfigurasi di `.env`.

## 5. Struktur Folder Project

Rekomendasi struktur jika memakai **FastAPI + React + SQLite**:

```text
osint-toolkit-local/
├── backend/
│   ├── app/
│   │   ├── main.py
│   │   ├── config.py
│   │   ├── database.py
│   │   ├── models/
│   │   │   ├── scan.py
│   │   │   ├── result.py
│   │   │   └── api_key.py
│   │   ├── schemas/
│   │   │   ├── scan_schema.py
│   │   │   └── result_schema.py
│   │   ├── routes/
│   │   │   ├── scans.py
│   │   │   ├── domain.py
│   │   │   ├── email.py
│   │   │   ├── username.py
│   │   │   ├── ip.py
│   │   │   └── reports.py
│   │   ├── services/
│   │   │   ├── dns_service.py
│   │   │   ├── whois_service.py
│   │   │   ├── subdomain_service.py
│   │   │   ├── email_service.py
│   │   │   ├── username_service.py
│   │   │   ├── ip_service.py
│   │   │   └── report_service.py
│   │   ├── workers/
│   │   │   └── scan_worker.py
│   │   └── utils/
│   │       ├── validators.py
│   │       ├── rate_limit.py
│   │       └── normalizer.py
│   ├── tests/
│   ├── requirements.txt
│   └── .env.example
│
├── frontend/
│   ├── src/
│   │   ├── api/
│   │   ├── components/
│   │   ├── pages/
│   │   │   ├── Dashboard.jsx
│   │   │   ├── NewScan.jsx
│   │   │   ├── ScanDetail.jsx
│   │   │   └── Settings.jsx
│   │   ├── styles/
│   │   └── main.jsx
│   ├── package.json
│   └── vite.config.js
│
├── reports/
│   └── generated/
│
├── data/
│   └── osint_toolkit.db
│
├── docs/
│   ├── project-plan.md
│   ├── architecture.md
│   ├── ethical-guidelines.md
│   └── api-reference.md
│
├── README.md
├── .gitignore
└── docker-compose.yml
```

## 6. Desain Arsitektur Sederhana

Komponen utama:

```text
User
  |
  v
Frontend Dashboard
  |
  v
Backend API
  |
  +--> Validation Layer
  |
  +--> OSINT Services
  |      ├── DNS Service
  |      ├── WHOIS Service
  |      ├── Email Service
  |      ├── Username Service
  |      ├── IP Service
  |      └── Report Service
  |
  +--> SQLite Database
  |
  +--> PDF Generator
```

Flow teknis:

1. User memasukkan target di frontend.
2. Frontend mengirim request ke backend.
3. Backend validasi target.
4. Backend menentukan modul lookup yang relevan.
5. Service menjalankan lookup pasif.
6. Hasil disimpan ke database.
7. Frontend menampilkan hasil scan.
8. User bisa export PDF.
9. Backend generate PDF dari data scan.

Mode eksekusi MVP:

```text
POST /api/scans
-> backend proses lookup
-> simpan hasil
-> return scan_id
```

Mode lanjut dengan worker async:

```text
POST /api/scans
-> create scan status: pending
-> worker proses lookup
-> update status: completed
```

## 7. Skema Database Awal

Database awal: SQLite.

### Tabel `scans`

Menyimpan metadata scan.

```sql
CREATE TABLE scans (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    target TEXT NOT NULL,
    target_type TEXT NOT NULL,
    status TEXT NOT NULL,
    risk_score INTEGER,
    created_at TEXT NOT NULL,
    completed_at TEXT,
    notes TEXT
);
```

Contoh `target_type`:

- `domain`
- `email`
- `username`
- `ip`

Contoh `status`:

- `pending`
- `running`
- `completed`
- `failed`

### Tabel `scan_results`

Menyimpan hasil setiap modul.

```sql
CREATE TABLE scan_results (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    scan_id INTEGER NOT NULL,
    module_name TEXT NOT NULL,
    status TEXT NOT NULL,
    summary TEXT,
    raw_data_json TEXT,
    created_at TEXT NOT NULL,
    FOREIGN KEY (scan_id) REFERENCES scans(id)
);
```

Contoh `module_name`:

- `dns_lookup`
- `whois_lookup`
- `subdomain_finder`
- `email_validation`
- `username_checker`
- `ip_reputation`

### Tabel `reports`

Menyimpan metadata file report.

```sql
CREATE TABLE reports (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    scan_id INTEGER NOT NULL,
    file_path TEXT NOT NULL,
    generated_at TEXT NOT NULL,
    FOREIGN KEY (scan_id) REFERENCES scans(id)
);
```

### Tabel `api_keys`

Opsional untuk menyimpan konfigurasi API key secara lokal.

```sql
CREATE TABLE api_keys (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    provider_name TEXT NOT NULL,
    key_name TEXT NOT NULL,
    encrypted_value TEXT NOT NULL,
    is_enabled INTEGER NOT NULL DEFAULT 1,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);
```

Untuk MVP, lebih sederhana memakai `.env` dulu daripada tabel `api_keys`.

Relasi utama:

```text
scans 1 --- many scan_results
scans 1 --- many reports
```

## 8. Endpoint API yang Dibutuhkan

### Scan

Create scan:

```http
POST /api/scans
```

Request:

```json
{
  "target": "example.com",
  "target_type": "domain"
}
```

Response:

```json
{
  "scan_id": 1,
  "status": "completed"
}
```

Get all scans:

```http
GET /api/scans
```

Get scan detail:

```http
GET /api/scans/{scan_id}
```

Delete scan:

```http
DELETE /api/scans/{scan_id}
```

### Domain

```http
POST /api/domain/recon
POST /api/domain/dns
POST /api/domain/whois
POST /api/domain/subdomains
```

### Email

```http
POST /api/email/validate
```

### Username

```http
POST /api/username/check
```

### IP

```http
POST /api/ip/lookup
POST /api/ip/reputation
```

### Report

Generate PDF:

```http
POST /api/reports/{scan_id}/generate
```

Download PDF:

```http
GET /api/reports/{report_id}/download
```

## 9. Alur User dari Input sampai Report

Alur utama:

1. User membuka dashboard lokal.
2. User klik `New Scan`.
3. User memilih tipe target: domain, email, username, atau IP.
4. User memasukkan target.
5. User memilih modul yang ingin dijalankan.
6. User klik `Start Scan`.
7. Backend melakukan validasi input.
8. Backend menjalankan lookup sesuai modul.
9. Hasil disimpan ke database.
10. Dashboard menampilkan ringkasan hasil.
11. User membuka detail hasil scan.
12. User klik `Generate PDF`.
13. Backend membuat report PDF.
14. User mengunduh report.

Contoh alur domain:

```text
Input:
example.com

Modules:
- DNS Lookup
- WHOIS Lookup
- Subdomain Finder

Output:
- DNS records
- Registrar info
- Name servers
- Found subdomains
- Report PDF
```

## 10. Roadmap Pengerjaan

### Version 0.1 - Project Foundation

Target:

- Setup repository
- Setup backend
- Setup frontend
- Setup SQLite
- Basic dashboard layout

Fitur:

- Health check API
- Database connection
- Basic scan model
- Frontend halaman dashboard kosong

Deliverable:

- App bisa jalan lokal
- Backend dan frontend tersambung

### Version 0.2 - Scan Management

Target:

- CRUD scan sederhana

Fitur:

- Create scan
- List scan
- Detail scan
- Delete scan
- Simpan target dan status

Deliverable:

- User bisa membuat dan melihat history scan

### Version 0.3 - Domain DNS Lookup

Target:

- Modul domain pertama

Fitur:

- DNS A record
- MX record
- NS record
- TXT record
- Simpan hasil JSON
- Tampilkan di UI

Deliverable:

- Domain scan menampilkan DNS records

### Version 0.4 - WHOIS Lookup

Target:

- Tambahkan WHOIS

Fitur:

- Registrar
- Created date
- Expiry date
- Name servers
- Domain status

Deliverable:

- Domain recon mulai terasa lengkap

### Version 0.5 - Email Validation

Target:

- Modul email dasar

Fitur:

- Format validation
- Domain extraction
- MX lookup
- Disposable check placeholder

Deliverable:

- Email bisa dianalisis secara pasif

### Version 0.6 - IP Lookup

Target:

- Modul IP dasar

Fitur:

- IP validation
- Reverse DNS
- Basic geolocation optional
- ASN placeholder

Deliverable:

- IP bisa dicek dengan hasil dasar

### Version 0.7 - Report PDF

Target:

- Export hasil scan

Fitur:

- Generate PDF
- Download PDF
- Report template
- Ethical disclaimer

Deliverable:

- Project mulai kuat untuk portfolio

### Version 0.8 - Subdomain Finder

Target:

- Passive subdomain discovery

Fitur:

- Integrasi `crt.sh` atau sumber publik sejenis
- Deduplicate subdomain
- DNS resolve ringan
- Simpan source

Deliverable:

- Domain recon lebih menarik

### Version 0.9 - Username Checker

Target:

- Cek username di beberapa platform publik

Fitur:

- GitHub
- GitLab
- Reddit
- Medium
- Rate limiting
- Timeout handling

Deliverable:

- Username investigation tersedia

### Version 1.0 - Portfolio Polish

Target:

- Project siap dipresentasikan

Fitur:

- UI rapi
- Error handling
- Loading states
- Settings API key optional
- README lengkap
- Screenshot
- Demo data
- Unit test dasar
- Docker Compose optional

Deliverable:

- Portfolio-ready project

## 11. Risiko Etis/Legal dan Batasan Fitur

Prinsip utama:

> Project hanya boleh mengambil data publik dan tidak boleh digunakan untuk akses tidak sah.

Batasan wajib:

- Tidak brute force login.
- Tidak password spraying.
- Tidak credential stuffing.
- Tidak bypass authentication.
- Tidak exploit vulnerability.
- Tidak port scanning agresif.
- Tidak directory brute forcing.
- Tidak mengakses data private.
- Tidak mengambil data dari balik login.
- Tidak scraping masif tanpa izin.
- Tidak mengabaikan robots.txt atau Terms of Service.
- Tidak menyimpan data sensitif tanpa alasan jelas.

### Username Checker

Risiko:

- Bisa dianggap scraping jika terlalu agresif.

Batasan:

- Gunakan rate limit.
- Platform terbatas.
- Pakai API resmi jika memungkinkan.
- Simpan hanya URL publik dan status ditemukan.

### Subdomain Finder

Risiko:

- Bruteforce subdomain bisa terlihat seperti recon aktif.

Batasan:

- MVP pakai passive source.
- Jangan gunakan wordlist besar.
- Jangan melakukan port scan otomatis.

### IP Reputation

Risiko:

- Data reputasi bisa salah atau out of date.

Batasan:

- Tampilkan source.
- Jangan membuat tuduhan final.
- Gunakan label seperti `Potential Risk`, bukan `Confirmed Malicious`.

### Report PDF

Risiko:

- Report bisa disalahartikan sebagai bukti final.

Batasan:

- Tambahkan disclaimer.
- Tampilkan tanggal scan.
- Tampilkan sumber data.
- Jelaskan bahwa hasil perlu verifikasi manual.

Disclaimer yang disarankan:

```text
This report is generated from publicly available information only.
The results are intended for educational, defensive, and authorized research purposes.
No authentication bypass, exploitation, brute force, or unauthorized access was performed.
Findings should be manually verified before being used for security decisions.
```

## 12. Saran Tech Stack Final

Rekomendasi utama:

```text
Backend: Python FastAPI
Frontend: React + Vite
Database: SQLite
PDF: WeasyPrint atau ReportLab
Worker: FastAPI BackgroundTasks dulu, Celery/RQ nanti
Styling: Tailwind CSS atau CSS biasa
```

### Alasan

FastAPI cocok karena:

- Mudah dibuat.
- Dokumentasi API otomatis lewat Swagger.
- Cocok untuk service-style backend.
- Banyak library OSINT, DNS, dan WHOIS di Python.
- Portfolio-friendly.

React + Vite cocok karena:

- Modern.
- Cepat untuk dashboard.
- Banyak opsi komponen UI.
- Mudah dipresentasikan sebagai portfolio software engineering.

SQLite cocok karena:

- Tidak perlu setup server database.
- Ideal untuk app lokal.
- Mudah dipindahkan ke PostgreSQL nanti.

Python worker optional cocok karena:

- Lookup OSINT sering lebih mudah di Python.
- Bisa dipisah dari API kalau project membesar.

## Final Recommendation

Gunakan stack ini untuk versi awal:

```text
FastAPI + React + SQLite
```

Pendekatan:

- Mulai dari fitur tanpa API key.
- Simpan hasil scan dalam JSON.
- Buat report PDF dari hasil database.
- API key dibuat optional setelah MVP stabil.

Urutan pengerjaan terbaik:

```text
0.1 Foundation
0.2 Scan CRUD
0.3 DNS Lookup
0.4 WHOIS Lookup
0.5 Email Validation
0.6 IP Lookup
0.7 PDF Report
0.8 Subdomain Finder
0.9 Username Checker
1.0 Polish
```

Project ini cukup kuat untuk portfolio karena menunjukkan:

- Backend API design
- Frontend dashboard
- Database modeling
- Integrasi external/public data
- Report generation
- Security ethics awareness
- Error handling dan rate limiting
- Dokumentasi teknis yang jelas
